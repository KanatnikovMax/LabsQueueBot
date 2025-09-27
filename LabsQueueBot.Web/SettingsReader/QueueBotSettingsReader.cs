using LabsQueueBot.Core.Settings;

namespace LabsQueueBot.Web.SettingsReader;

public static class QueueBotSettingsReader
{
    public static TelegramBotSettings Read(IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection("BotSettings");
        
        var token = section.GetValue<string>("Token");
        var url = section.GetValue<string>("Url");
        var unionTime = section.GetValue<string>("UnionTimeUtc");
        var localOffset = section.GetValue<int>("LocalUtcOffset");
        var stateUpdateTimeoutInMinutes = section.GetValue<int>("StateUpdateTimeoutInMinutes");
        var stateAllowedIntervalInMinutes = section.GetValue<int>("StateAllowedIntervalInMinutes");
        
        var settings = new TelegramBotSettings
        {
            PrivilegedChatId = [],
            AdminChatId = [],
            Token = token,
            Url = url,
            UnionTimeUtc = TimeOnly.Parse(unionTime).ToTimeSpan(),
            LocalUtcOffset = localOffset,
            CleanerJobTimeoutInMinutes = 1,
            StateUpdateTimeoutInMinutes = stateUpdateTimeoutInMinutes,
            StateAllowedIntervalInMinutes = stateAllowedIntervalInMinutes
        };

        section.GetSection("PrivilegedChatId").Bind(settings.PrivilegedChatId);
        section.GetSection("AdminChatId").Bind(settings.AdminChatId);

        return settings;
    }
}