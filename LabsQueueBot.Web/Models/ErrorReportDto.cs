using Telegram.Bot.Types;

namespace LabsQueueBot.Web.Models;

public class ErrorReportDto
{
    public required string? ThrownException { get; init; }
    public required string? Message { get; init; }

    public required IEnumerable<string>? StackTrace { get; init; }

    public required Update? LastUpdate { get; init; }
}