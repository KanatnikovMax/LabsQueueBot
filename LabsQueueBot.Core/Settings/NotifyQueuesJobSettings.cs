namespace LabsQueueBot.Core.Settings;

public class NotifyQueuesJobSettings
{
    public required bool IsEnabled { get; init; }
    public required TimeSpan NotificationTimeUtc { get; init; }
}