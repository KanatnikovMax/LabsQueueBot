using Microsoft.Extensions.Configuration;

namespace LabsQueueBot.Settings;

public static class LabsQueueBotSettingsReader
{
    public static LabsQueueBotSettings Read(IConfiguration configuration)
    {
        var settings = new LabsQueueBotSettings()
        {
            AdminChatTgIds = new List<long>(),
            LogChatTgIds = new List<long>()
        };
        
        settings.BotToken = configuration.GetValue<string>("BotToken");
        configuration.GetSection("AdminChatTgIds").Bind(settings.AdminChatTgIds);
        configuration.GetSection("LogChatTgIds").Bind(settings.LogChatTgIds);
        settings.TimeForNotification = configuration.GetValue<string>("TimeForNotification");
        
        return settings;
    }
}