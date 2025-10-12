namespace LabsQueueBot.Core.Settings;

public class JobsSettings
{
    public required CleanerJobSettings CleanerJobSettings { get; init; }
    public required UnionJobSettings UnionJobSettings { get; init; }
    public required NotifyQueuesJobSettings NotifyQueuesJobSettings { get; init; }
}