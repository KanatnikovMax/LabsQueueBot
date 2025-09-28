using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.Services;
using LabsQueueBot.Web.Services.Implementation;
using Telegram.Bot;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class ServicesConfigurator
{
    public static IServiceCollection AddCommonServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var settings = configuration.GetRequiredSection(nameof(TelegramBotSettings)).Get<TelegramBotSettings>();
        ArgumentNullException.ThrowIfNull(settings);
        
        serviceCollection.AddSingleton<ITelegramBotClient>(new TelegramBotClient(settings.Token));
        
        serviceCollection.AddSingleton<IQueueInfoNotificationService, QueueInfoNotificationService>();
        serviceCollection.AddSingleton<IAdminNotificationService, AdminNotificationService>();

        return serviceCollection;
    }
}