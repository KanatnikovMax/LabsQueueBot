using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.Providers;
using LabsQueueBot.Web.Services;
using Microsoft.Extensions.Options;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Jobs;

public class NotifyQueuesJob(
    IServiceScopeFactory scopeFactory,
    IWeekProvider weekProvider,
    IQueueInfoNotificationService notificationService,
    IOptions<TelegramBotSettings> botOptions,
    IOptions<NotifyQueuesJobSettings> jobOptions,
    ILogger logger) : JobBase(logger)
{
    private const string NotificationMessage =
        """
        Пожалуйста, убедись, что не находишься в очередях по этим дисциплинам без необходимости:
        {0}
        """;
    
    protected override TimeSpan JobDelayBeforeStart => 
        DateTime.UtcNow.TimeOfDay > jobOptions.Value.NotificationTimeUtc
            ? DateTime.UtcNow.TimeOfDay - jobOptions.Value.NotificationTimeUtc
            : jobOptions.Value.NotificationTimeUtc - DateTime.UtcNow.TimeOfDay;
    protected override TimeSpan JobTimeout => TimeSpan.FromDays(1);
    
    protected override async Task JobBody(CancellationToken cancellationToken)
    {
        var dayOfWeek = WeekDaysHelper.ToWeekDay(DateTime.UtcNow.AddHours(botOptions.Value.LocalUtcOffset).AddDays(1).DayOfWeek);
        
        if (dayOfWeek == WeekDays.Monday) // обновляем флаг, если будем упоминать о следующей неделе (числитель <==> знаменатель)
            weekProvider.SwitchWeekNumerate();

        await using var scope = scopeFactory.CreateAsyncScope();
        var subjectsManagementService = scope.ServiceProvider.GetRequiredService<ISubjectsManagementService>();

        var subjects = (await subjectsManagementService.GetByDayOfWeekInTimetable(
                dayOfWeek: dayOfWeek,
                isNumWeek: weekProvider.IsNumWeek,
                cancellationToken: cancellationToken))
            .Where(x => x.Queue.Length > 0)
            .ToList();
        
        if (subjects.Count > 0)
        {
            var usersBySubjectsQueues = subjects
                .Select(x => new { x.Id, x.SubjectName, x.Queue })
                .ToDictionary(x => (x.Id, x.SubjectName), x => x.Queue);

            var usersWithSubjects = GetSubjectsByUsers(usersBySubjectsQueues);
            
            var usersWithMessages = usersWithSubjects
                .Select(x => 
                    new 
                    { 
                        UserId = x.Key, 
                        Subjects = string.Format(NotificationMessage, string.Join("\n", x.Value))
                    })
                .ToDictionary(x => x.UserId, x => x.Subjects);
            
            await notificationService.NotifyUsersWithMessages(usersWithMessages, cancellationToken);
        }
    }

    private static Dictionary<long, List<string>> GetSubjectsByUsers(Dictionary<(int SubjectId, string SubjectName), long[]> usersBySubjectsQueues)
    {
        var usersWithSubjectsNames = new Dictionary<long, List<string>>();

        foreach (var subjectQueueInfo in usersBySubjectsQueues)
        {
            foreach (var userId in subjectQueueInfo.Value)
            {
                if (usersWithSubjectsNames.TryGetValue(userId, out var value))
                {
                    value.Add(subjectQueueInfo.Key.SubjectName);
                }
                else
                {
                    usersWithSubjectsNames.Add(userId, [subjectQueueInfo.Key.SubjectName]);
                }
            }
        }

        return usersWithSubjectsNames;
    }
}