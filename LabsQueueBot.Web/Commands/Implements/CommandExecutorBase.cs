using System.Diagnostics;
using System.Text.Json;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Utils;
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
    private const string DebugMessage = "Command {0} execution by Update:\n{1}";
    private const string InfoMessage = "UpdateId: {0} Command: {1} UserId: {2}";
    private const string ProfilingMessage = "UpdateId: {0} Command: {1} Ellapsed: {2}ms Result: {3}";
    
    private readonly Stopwatch _stopwatch = new();
    
    public abstract string? Type { get; }
    public abstract string Name { get; }
    public abstract IReadOnlyCollection<(UserState State, UpdateType Type)> Allows { get; }
    public abstract Role AcceptRole { get; }
    public abstract string? Definition { get; }

    public async Task Execute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        // TODO почему не пишется дебаг
        logger.Debug(DebugMessage, Name, JsonSerializer.Serialize(update));
        logger.Information(InfoMessage, update.Id, Name, user.Id);

        var isSuccess = false;
        
        _stopwatch.Start();
        try
        {
            isSuccess = await InternalExecute(botClient, update, user, cancellationToken);

            if (!isSuccess)
            {
                // если при  UserState.None, UserState.ChooseGroup или UserState.AddGroup получены Update не ожидаемого типа
                if (!await BotClientUtils.DeleteUpdate(botClient, user.Id, update, cancellationToken))
                {
                    // в случае если получили невозможный Update (не Message и не CallbackQuery) - игнорируем его
                    var updateString = JsonSerializer.Serialize(update);
                    logger.Warning("Update.MessageId is null\n\n{updateString}", updateString);
                }
            }
        }
        finally
        {
            _stopwatch.Stop();
            logger.Information(ProfilingMessage, update.Id, GetType(), _stopwatch.Elapsed.Milliseconds, isSuccess ? Success : Error);
            //_stopwatch.Reset();
        }
    }
    
    protected abstract Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken);
}