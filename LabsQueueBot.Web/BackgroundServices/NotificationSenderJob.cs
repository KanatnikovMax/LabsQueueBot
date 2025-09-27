using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.Services;

namespace LabsQueueBot.Web.BackgroundServices;

public class NotificationSenderJob( // TODO переделать в QueueWaitingUnionJob и оставить отправку уведомлений
    IQueueInfoNotificationService queueInfoNotificationService,
    QueueBotSettings settings)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var delay = DateTime.UtcNow.TimeOfDay > settings.UnionNotificationTimeUtc
            ? DateTime.UtcNow.TimeOfDay - settings.UnionNotificationTimeUtc
            : settings.UnionNotificationTimeUtc - DateTime.UtcNow.TimeOfDay;
        
        await Task.Delay(delay, cancellationToken);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await queueInfoNotificationService.NotifyAll(cancellationToken);

            await Task.Delay(TimeSpan.FromHours(24), cancellationToken);
        }
    }
}