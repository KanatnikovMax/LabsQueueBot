using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.Services;
using Microsoft.Extensions.Options;

namespace LabsQueueBot.Web.BackgroundServices;

public class QueueWaitingUnionJob( // TODO проверить
    IServiceScopeFactory scopeFactory,
    IQueueInfoNotificationService queueInfoNotificationService,
    IOptions<TelegramBotSettings> options)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var delay = DateTime.UtcNow.TimeOfDay > options.Value.UnionTimeUtc
            ? DateTime.UtcNow.TimeOfDay - options.Value.UnionTimeUtc
            : options.Value.UnionTimeUtc - DateTime.UtcNow.TimeOfDay;
        
        await Task.Delay(delay, cancellationToken);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var currentDayOfWeek = WeekDaysHelper.ToWeekDay(DateTime.UtcNow.DayOfWeek);

            await using var scope = scopeFactory.CreateAsyncScope();
            var subjectsManagementService = scope.ServiceProvider.GetRequiredService<ISubjectsManagementService>();

            var subjects = (await subjectsManagementService.GetByDayOfWeekInTimetable(
                    dayOfWeek: currentDayOfWeek, 
                    isNumWeek: true, 
                    cancellationToken: cancellationToken))
                .Select(s => s.Id)
                .ToList();
            if (subjects.Count != 0)
            {
                var unionService = scope.ServiceProvider.GetRequiredService<IRandomizeUnionWaitingService>();
                
                await UnionAndNotify(subjects, unionService, cancellationToken);
            }
            
            await Task.Delay(TimeSpan.FromHours(24), cancellationToken);
        }
    }

    private Task UnionAndNotify(IReadOnlyCollection<int> subjects, IRandomizeUnionWaitingService unionService, CancellationToken cancellationToken)
    {
        var tasks = subjects
            .Select(s => Task.Run(async () => 
                { 
                    var subject = await unionService.RandomizeAndUnionWaitingBySubject(s, cancellationToken);
                    if (subject == null)
                        return;
                    
                    await queueInfoNotificationService.NotifyGroupBySubject(
                        course: subject.CourseNumber, 
                        group: subject.GroupNumber, 
                        subjectName: subject.SubjectName, 
                        queue: subject.Queue.ToList(), 
                        waiting: subject.Waiting.ToList(), 
                        cancellationToken: cancellationToken); 
                }, cancellationToken))
            .ToArray();

        Task.WaitAll(tasks, cancellationToken);
        return Task.CompletedTask;
    }
}