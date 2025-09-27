namespace LabsQueueBot.Core.Settings;

public class QueueBotSettings
{
    public string BotToken { get; init; }
    public string BotUrl { get; init; }
    public string ConnectionString { get; init; }
    public List<long> PrivilegedChatId { get; init; }
    public List<long> AdminChatId { get; init; }
    public TimeSpan UnionNotificationTimeUtc { get; init; }
    public TimeSpan LocalUtcOffset { get; init; }
    public int UserStateUpdateTimeoutInMinutes { get; init; }
    public int UserStateAllowedIntervalInMinutes { get; init; }
}