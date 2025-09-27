using LabsQueueBot.Core.Settings;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class ApplicationSettingsConfigurator
{
    public static IServiceCollection ConfigureSettings(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<PostgreSqlSettings>(configuration.GetRequiredSection(nameof(PostgreSqlSettings)));
        serviceCollection.Configure<TelegramBotSettings>(configuration.GetRequiredSection(nameof(TelegramBotSettings)));
        serviceCollection.Configure<CommansSettings>(configuration.GetRequiredSection(nameof(CommandsSettings)));
        
        return serviceCollection;
    }
}