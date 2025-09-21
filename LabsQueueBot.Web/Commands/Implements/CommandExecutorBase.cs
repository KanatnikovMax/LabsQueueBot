using System.Diagnostics;
using System.Text.Json;
using LabsQueueBot.Core.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = LabsQueueBot.DataAccess.Entities.User;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Commands.Implements;

public abstract class CommandExecutorBase(ILogger logger) : ICommandExecutor
{
    private const string InfoMessage = "Command {0} execution by Update:\n{1}";
    private const string ProfilingMessage = "UpdateId: {0} Command: {1} Ellapsed: {2}ms";
    
    private readonly Stopwatch _stopwatch = new();
    
    public abstract string? Type { get; }
    public abstract string Name { get; }
    public abstract IReadOnlyCollection<UserState> States { get; }
    public abstract Role AcceptRole { get; }
    public abstract string? Definition { get; }

    public async Task Execute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        // TODO почему не пишется дебаг
        logger.Debug(InfoMessage, Name, JsonSerializer.Serialize(update));
        
        _stopwatch.Start();
        try
        {
            await InternalExecute(botClient, update, user, cancellationToken);
        }
        finally
        {
            _stopwatch.Stop();
            logger.Information(ProfilingMessage, update.Id, GetType(), _stopwatch.Elapsed.Milliseconds);
            _stopwatch.Reset();
        }
    }
    
    protected abstract Task InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken);
}