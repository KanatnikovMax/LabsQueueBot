using LabsQueueBot.Core.Settings;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class ApplicationSettingsConfigurator
{
    public static IServiceCollection ConfigureSettings(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection
            .Configure<PostgreSqlSettings>(configuration.GetRequiredSection(nameof(PostgreSqlSettings)))
            .Configure<CommandsSettings>(configuration.GetRequiredSection(nameof(CommandsSettings)))
            .Configure<TelegramBotSettings>(configuration.GetRequiredSection(nameof(TelegramBotSettings)))
            .PostConfigure<TelegramBotSettings>(settings =>
            {
                var section = configuration.GetRequiredSection(nameof(TelegramBotSettings));
                settings.AdminChatId = section.GetLongArray(nameof(TelegramBotSettings.AdminChatId));
                settings.PrivilegedChatId = section.GetLongArray(nameof(TelegramBotSettings.PrivilegedChatId));
            });
        
        return serviceCollection;
    }
    
    private static IEnumerable<long> GetLongArray(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrEmpty(value))
            return [];

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(long.Parse);
    }
}