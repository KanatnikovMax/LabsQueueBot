using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Repository.Repository;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Commands.Implements;

public class StopCommandExecutor(
    ISubjectsManagementService subjectsManagementService,
    IUserRepository userRepository,
    IOptions<CommandsSettings> options,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string ByeByeMessage = "Прощай, мой друг";
    
    public override string Type => options.Value.Stop.Type;
    public override string Name => options.Value.Stop.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [];
    public override Role AcceptRole => Role.Nobody;
    public override string Definition => options.Value.Stop.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        await subjectsManagementService.DeleteUserFromSubjectsQueues(user.Id, user.CourseNumber, user.GroupNumber, cancellationToken);

        await userRepository.DeleteAsync(user, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: ByeByeMessage,
            cancellationToken: cancellationToken);

        return true;
    }
}