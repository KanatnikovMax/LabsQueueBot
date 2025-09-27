using LabsQueueBot.Core.Settings;

namespace LabsQueueBot.Web.SettingsReader;

public static class QueueBotSettingsReader
{
    public static QueueBotSettings Read(IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("QueueBotDbContext");
        
        var section = configuration.GetRequiredSection("BotSettings");
        
        var token = section.GetValue<string>("Token");
        var url = section.GetValue<string>("Url");
        var unionTime = section.GetValue<string>("UnionTimeUtc");
        var localOffset = section.GetValue<int>("LocalUtcOffset");
        var stateUpdateTimeoutInMinutes = section.GetValue<int>("StateUpdateTimeoutInMinutes");
        var stateAllowedIntervalInMinutes = section.GetValue<int>("StateAllowedIntervalInMinutes");
        
        var settings = new QueueBotSettings
        {
            PrivilegedChatId = [],
            AdminChatId = [],
            Token = token,
            Url = url,
            ConnectionString = connectionString,
            UnionNotificationTimeUtc = TimeOnly.Parse(unionTime).ToTimeSpan(),
            LocalUtcOffset = TimeSpan.FromHours(localOffset),
            StateUpdateTimeoutInMinutes = stateUpdateTimeoutInMinutes,
            StateAllowedIntervalInMinutes = stateAllowedIntervalInMinutes
        };

        section.GetSection("PrivilegedChatId").Bind(settings.PrivilegedChatId);
        section.GetSection("AdminChatId").Bind(settings.AdminChatId);
        
        settings.PrivilegedChatId.RemoveAll(x => settings.AdminChatId.Contains(x));

        return settings;
    }
}