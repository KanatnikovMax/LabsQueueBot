using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = LabsQueueBot.DataAccess.Entities.User;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Commands.Implements;

public class StopCommandExecutor(
    IUserManagementService userManagementService,
    CommandsSettings commandsSettings,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string ByeByeMessage = "Прощай, мой друг";
    
    public override string Type => commandsSettings.StopCommand.Type;
    public override string Name => commandsSettings.StopCommand.Name;
    public override IReadOnlyCollection<UserState> States => [];
    public override Role AcceptRole => Role.Nobody;
    public override string Definition => commandsSettings.StopCommand.Definition;
    
    protected override async Task InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        await userManagementService.DeleteUser(user, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: ByeByeMessage,
            cancellationToken: cancellationToken);
    }
}