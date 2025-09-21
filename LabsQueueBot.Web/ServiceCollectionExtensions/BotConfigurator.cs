using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.BackgroundServices;
using LabsQueueBot.Web.Providers;
using LabsQueueBot.Web.UpdateHandler;
using Telegram.Bot;
using Telegram.Bot.Polling;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class BotConfigurator
{
    public static IServiceCollection AddTelegramBotServices(this IServiceCollection serviceCollection, QueueBotSettings settings, CancellationToken cancellationToken)
    {
        serviceCollection.AddSingleton<IUpdateHandler, QueueBotUpdateHandler>(x => new QueueBotUpdateHandler(
            x.GetRequiredService<ILogger>(),
            x.GetRequiredService<INotificationProvider>(),
            x.GetRequiredService<IServiceScopeFactory>()));

        serviceCollection.AddHostedService<LabsQueueBotService>(x => new LabsQueueBotService(
            x.GetRequiredService<ILogger>(),
            x.GetRequiredService<ITelegramBotClient>(),
            x.GetRequiredService<IUpdateHandler>(),
            settings));
        
        // serviceCollection.AddHostedService<NotificationSenderService>(x => new NotificationSenderService(
        //     x.GetRequiredService<INotificationProvider>(),
        //     settings));
        
        // // джоба обновления состояний и чатов пользователей во время бездействия
        // serviceCollection.AddHostedService<UsersCleanerJob>(x => new UsersCleanerJob(
        //     x.GetRequiredService<IUserCleanerService>(),
        //     settings,
        //     x.GetRequiredService<ILogger>()));
        
        return serviceCollection;
    }
}