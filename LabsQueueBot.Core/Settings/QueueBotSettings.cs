namespace LabsQueueBot.Core.Settings;

public class QueueBotSettings
{
    public string Token { get; init; }
    public string Url { get; init; }
    public string ConnectionString { get; init; }
    public List<long> PrivilegedChatId { get; init; }
    public List<long> AdminChatId { get; init; }
    public TimeSpan UnionNotificationTimeUtc { get; init; }
    public TimeSpan LocalUtcOffset { get; init; }
    public int StateUpdateTimeoutInMinutes { get; init; }
    public int StateAllowedIntervalInMinutes { get; init; }
}