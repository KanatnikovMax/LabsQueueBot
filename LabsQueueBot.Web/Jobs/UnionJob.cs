using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.Services;
using Microsoft.Extensions.Options;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Jobs;

public class UnionJob(
    IServiceScopeFactory scopeFactory,
    IQueueInfoNotificationService queueInfoNotificationService,
    IOptions<UnionJobSettings> options,
    ILogger logger) : JobBase(logger)
{
    protected override TimeSpan JobDelayBeforeStart =>
        DateTime.UtcNow.TimeOfDay > options.Value.UnionTimeUtc
        ? DateTime.UtcNow.TimeOfDay - options.Value.UnionTimeUtc
        : options.Value.UnionTimeUtc - DateTime.UtcNow.TimeOfDay;
    protected override TimeSpan JobTimeout => TimeSpan.FromDays(1);

    protected override async Task JobBody(CancellationToken cancellationToken)
    {
        var currentDayOfWeek = WeekDaysHelper.ToWeekDay(DateTime.UtcNow.DayOfWeek);

        await using var scope = scopeFactory.CreateAsyncScope();
        var subjectsManagementService = scope.ServiceProvider.GetRequiredService<ISubjectsManagementService>();

        var subjects = (await subjectsManagementService.GetByDayOfWeekInTimetable(
                dayOfWeek: currentDayOfWeek, 
                isNumWeek: true, // TODO: обращать внимание на числитель/знаменатель
                cancellationToken: cancellationToken))
            .Where(s => s.Waiting.Length > 0)
            .ToList();
        
        if (subjects.Count > 0)
        {
            var subjectsIds = subjects.Select(x => x.Id).ToList();
            var unionService = scope.ServiceProvider.GetRequiredService<IRandomizeUnionWaitingService>();

            var unionTask = unionService.RandomizeAndUnionWaitingByBatch(subjectsIds, cancellationToken);
            
            var groups = (await unionTask)
                .Select(s => (s.CourseNumber, s.GroupNumber))
                .Distinct()
                .ToList();
            await NotifyGroups(groups, cancellationToken);
        }
    }

    private Task NotifyGroups(IReadOnlyCollection<(byte Course, byte Group)> groups, CancellationToken cancellationToken)
    {
        var tasks = groups
            .Select(g => 
                Task.Run(() =>
                        queueInfoNotificationService.NotifyGroup(g.Course, g.Group, cancellationToken), 
                    cancellationToken))
            .ToArray();

        Task.WaitAll(tasks, cancellationToken);
        return Task.CompletedTask;
    }
}