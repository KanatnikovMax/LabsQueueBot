using System.Diagnostics;
using System.Text.Json;
using LabsQueueBot.Core.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Commands.Implements;

public abstract class CommandExecutorBase(ILogger logger) : ICommandExecutor
{
    private const string Success = "success";
    private const string Error = "failure";
    private const string InfoMessage = "UpdateId: {0} CommandExecutor: {1} UserId: {2}";
    private const string DebugMessage = "UpdateId: {0} Message: {1} CallbackQuery: {2}";
    private const string ProfilingMessage = "UpdateId: {0} CommandExecutor: {1} Ellapsed ms: {2} Result: {3}";
    
    private readonly Stopwatch _stopwatch = new();
    
    public abstract string? Type { get; }
    public abstract string Name { get; }
    public abstract IReadOnlyCollection<(UserState State, UpdateType Type)> Allows { get; }
    public abstract Role AcceptRole { get; }
    public abstract string? Definition { get; }
    
    protected abstract Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken);

    public async Task Execute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        // TODO поправить костыльное debug-логгирование
        logger.Debug(DebugMessage, update.Id, JsonSerializer.Serialize(update.Message?.Text), JsonSerializer.Serialize(update.CallbackQuery?.Data?.Normalize()));
        logger.Information(InfoMessage, update.Id, GetType().Name, user.Id);

        var isSuccess = false;
        
        _stopwatch.Start();
        try
        {
            isSuccess = await InternalExecute(botClient, update, user, cancellationToken);
        }
        catch
        {
            isSuccess = false;
            throw;
        }
        finally
        {
            _stopwatch.Stop();
            logger.Information(ProfilingMessage, update.Id, GetType().Name, _stopwatch.Elapsed.Milliseconds, isSuccess ? Success : Error);
        }
    }
}