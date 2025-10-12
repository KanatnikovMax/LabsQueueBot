namespace LabsQueueBot.Core.Settings;

public class UnionJobSettings
{
    public required bool IsEnabled { get; init; }
    public required TimeSpan UnionTimeUtc { get; init; }
}