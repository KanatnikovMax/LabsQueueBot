using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.Jobs;
using LabsQueueBot.Web.UpdateHandler;
using Telegram.Bot.Polling;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class TelegramBotConfigurator
{
    public static IServiceCollection AddTelegramBotServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var botSettings = configuration.GetRequiredSection(nameof(TelegramBotSettings)).Get<TelegramBotSettings>();
        ArgumentNullException.ThrowIfNull(botSettings);
        
        var jobsSettings = configuration.GetRequiredSection(nameof(JobsSettings)).Get<JobsSettings>();
        ArgumentNullException.ThrowIfNull(jobsSettings);

        if (jobsSettings.UnionJobSettings.IsEnabled && jobsSettings.NotifyQueuesJobSettings.IsEnabled && jobsSettings.NotifyQueuesJobSettings.NotificationTimeUtc > jobsSettings.UnionJobSettings.UnionTimeUtc
            || TimeOnly.FromTimeSpan(jobsSettings.UnionJobSettings.UnionTimeUtc).AddHours(botSettings.LocalUtcOffset) < TimeOnly.FromTimeSpan(jobsSettings.NotifyQueuesJobSettings.NotificationTimeUtc).AddHours(botSettings.LocalUtcOffset))
            throw new ArgumentException("NotifyQueuesJob срабатывает позже, чем UnionJob");
        
        serviceCollection.AddSingleton<IUpdateHandler, QueueBotUpdateHandler>();
        
        serviceCollection.AddHostedService<LabsQueueBotStartingJob>();
        
        if (jobsSettings.UnionJobSettings.IsEnabled)
            serviceCollection.AddHostedService<UnionJob>();
        if (jobsSettings.NotifyQueuesJobSettings.IsEnabled)
            serviceCollection.AddHostedService<NotifyQueuesJob>();
        
        // джоба обновления состояний и чатов пользователей во время бездействия
        serviceCollection.AddHostedService<UsersCleanerJob>(); 
        
        return serviceCollection;
    }
}