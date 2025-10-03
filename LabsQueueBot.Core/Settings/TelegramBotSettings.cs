using LabsQueueBot.Core.Constants;

namespace LabsQueueBot.Core.Settings;

public class TelegramBotSettings
{
    public required string Token { get; init; }
    public required string Url { get; init; }
    public required IEnumerable<long> PrivilegedChatId { get; init; } = [];
    public required IEnumerable<long> AdminChatId { get; init; } = [];
    public required TimeSpan UnionTimeUtc { get; init; }
    public required int LocalUtcOffset { get; init; }
    public required int CleanerJobTimeoutInMinutes { get; init; }
    public required int StateUpdateTimeoutInMinutes { get; init; }
    public required int StateAllowedIntervalInMinutes { get; init; }
    public required int MaxBanTimeoutInDays { get; init; } = GlobalConstants.DefaultMaxBanTimeoutInDays;
}