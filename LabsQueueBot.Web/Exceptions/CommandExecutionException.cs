using Telegram.Bot.Types;

namespace LabsQueueBot.Web.Exceptions;

public class CommandExecutionException(Update u, Exception e) : Exception
{
    public Update LastUpdate { get; init; } = u;
    public Exception ThrownException { get; init; } = e;
}