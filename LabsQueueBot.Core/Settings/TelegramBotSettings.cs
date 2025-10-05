using LabsQueueBot.Core.Constants;

namespace LabsQueueBot.Core.Settings;

public class TelegramBotSettings
{
    public required string Token { get; init; }
    public required string Url { get; init; }
    public required IEnumerable<long> PrivilegedChatId { get; set; } = [];
    public required IEnumerable<long> AdminChatId { get; set; } = [];
    public required TimeSpan UnionTimeUtc { get; init; }
    public required int LocalUtcOffset { get; init; }
    public required int CleanerJobTimeoutInMinutes { get; init; }
    public required int ClearStateTimeoutInMinutes { get; init; }
    public required int MaxBanTimeoutInDays { get; init; } = GlobalConstants.DefaultMaxBanTimeoutInDays;
}