using LabsQueueBot.Core.Settings;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Repository.Repository.Implements;
using LabsQueueBot.Web.Providers;
using LabsQueueBot.Web.Providers.Services;
using Telegram.Bot;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class ServicesConfigurator
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection, QueueBotSettings settings)
    {
        serviceCollection.AddScoped<IUserRepository, UserRepository>();
        serviceCollection.AddScoped<ISubjectRepository, SubjectRepository>();
        serviceCollection.AddScoped<ISerialNumberRepository, SerialNumberRepository>();
        
        serviceCollection.AddSingleton<ITelegramBotClient>(x => new TelegramBotClient(
            settings.BotToken));
        
        serviceCollection.AddSingleton<INotificationProvider, NotificationProvider>();

        return serviceCollection;
    }
}