using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.Services;
using Microsoft.Extensions.Options;

namespace LabsQueueBot.Web.BackgroundServices;

public class NotificationSenderJob( // TODO переделать в QueueWaitingUnionJob и оставить отправку уведомлений
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
            await queueInfoNotificationService.NotifyAll(cancellationToken);

            await Task.Delay(TimeSpan.FromHours(24), cancellationToken);
        }
    }
}