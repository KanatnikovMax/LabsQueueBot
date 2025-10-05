using LabsQueueBot.Web.BackgroundServices;
using LabsQueueBot.Web.UpdateHandler;
using Telegram.Bot.Polling;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class TelegramBotConfigurator
{
    public static IServiceCollection AddTelegramBotServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IUpdateHandler, QueueBotUpdateHandler>();
        
        serviceCollection.AddHostedService<LabsQueueBotService>();
        
        // TODO включить union-job
        // serviceCollection.AddHostedService<QueueWaitingUnionJob>();
        
        // джоба обновления состояний и чатов пользователей во время бездействия
        serviceCollection.AddHostedService<UsersCleanerJob>(); 
        
        return serviceCollection;
    }
}