using LabsQueueBot.Core.Settings;

namespace LabsQueueBot.Web.Settings.QueueBot;

public static class QueueBotSettingsReader
{
    public static QueueBotSettings Read(IConfiguration configuration)
    {
        var queueBotContext = configuration.GetValue<string>("QueueBotDbContext");
        
        var section = configuration.GetSection("LabsQueueBot");
        
        var botToken = section.GetValue<string>("BotToken");
        var botUrl = section.GetValue<string>("BotUrl");
        var unionNotificationTime = section.GetValue<string>("UnionNotificationTimeUtc");
        var userStateUpdateTimeoutInMinutes = section.GetValue<int>("UserStateUpdateTimeoutInMinutes");
        var userStateAllowedIntervalInMinutes = section.GetValue<int>("UserStateAllowedIntervalInMinutes");
        
        var settings = new QueueBotSettings
        {
            PrivilegedChatId = [],
            AdminChatId = [],
            BotToken = botToken,
            BotUrl = botUrl,
            QueueBotDbContext = queueBotContext,
            UnionNotificationTimeUtc = TimeOnly.Parse(unionNotificationTime).ToTimeSpan(),
            UserStateUpdateTimeoutInMinutes = userStateUpdateTimeoutInMinutes,
            UserStateAllowedIntervalInMinutes = userStateAllowedIntervalInMinutes
        };

        section.GetSection("PrivilegedChatId").Bind(settings.PrivilegedChatId);
        section.GetSection("AdminChatId").Bind(settings.AdminChatId);
        
        settings.PrivilegedChatId.RemoveAll(x => settings.AdminChatId.Contains(x));

        return settings;
    }
}