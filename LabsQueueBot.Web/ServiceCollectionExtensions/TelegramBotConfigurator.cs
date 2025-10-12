using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.Jobs;
using LabsQueueBot.Web.UpdateHandler;
using Telegram.Bot.Polling;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class TelegramBotConfigurator
{
    public static IServiceCollection AddTelegramBotServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddSingleton<IUpdateHandler, QueueBotUpdateHandler>();
        
        serviceCollection.AddHostedService<LabsQueueBotStartingJob>();

        var jobsSettings = configuration.GetRequiredSection(nameof(JobsSettings)).Get<JobsSettings>();
        ArgumentNullException.ThrowIfNull(jobsSettings);
        
        if (jobsSettings.UnionJobSettings.IsEnabled)
            serviceCollection.AddHostedService<UnionJob>();
        if (jobsSettings.NotifyQueuesJobSettings.IsEnabled)
            serviceCollection.AddHostedService<NotifyQueuesJob>();
        
        // джоба обновления состояний и чатов пользователей во время бездействия
        serviceCollection.AddHostedService<UsersCleanerJob>(); 
        
        return serviceCollection;
    }
}