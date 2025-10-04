using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Validators;
using LabsQueueBot.Repository.Repository;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class GrantCommandExecutor(
    IUserRepository userRepository,
    IOptions<CommandsSettings> commandsOptions,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string EnterGrantInfoMessage = "Введите имя пользователя, которому хотите повысить роль";
    private const string InvalidUsernameMessage = "Введено некорректное имя пользователя:\n{0}";
    private const string UserToGrantNotFoundMessage = "Пользователя с именем {0} не существует";
    private const string YouAreWhoYouAre = "Повысить роль самому себе нельзя. Хитрюга!";
    private const string NotAccessibleUserRole = "Вы не можете повысить роль пользователю {0}";
    private const string TopUserRoleMessage = "Пользователь уже имеет наивысшую роль - {0}";
    private const string SuccessGrantMessage = "Роль пользователя {0} успешно повышена. Новая роль - {1}";
    private const string GrantedMessage = "Ваша роль повышена. Новая роль - {0}";
    
    public override string Type => commandsOptions.Value.Grant.Type;
    public override string Name => commandsOptions.Value.Grant.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.Grant, UpdateType.Message) ];
    public override Role AcceptRole => Role.Privileged;
    public override string Definition => commandsOptions.Value.Grant.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        switch (user.State)
        {
            case UserState.None:
            {
                await SendEnterGrantInfo(botClient, user, cancellationToken);
                isSuccess = true;
                break;
            }
            case UserState.Grant:
            {
                await GrantChosenUser(botClient, update, user, cancellationToken);
                isSuccess = true;
                break;
            }
        }
        return isSuccess;
    }

    private async Task SendEnterGrantInfo(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.Grant;
        await userRepository.SaveAsync(user, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: EnterGrantInfoMessage,
            cancellationToken: cancellationToken);
    }

    private async Task GrantChosenUser(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);

        var username = update.Message?.Text;
        
        var validationResult = UserInfoValidator.ValidateUsername(username);
        if (validationResult != null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(InvalidUsernameMessage, validationResult),
                cancellationToken: cancellationToken);
            return;
        }
        
        var userToGrant = (await userRepository.GetByConditionAsync(u => u.Username == username, cancellationToken: cancellationToken))
            .FirstOrDefault();
        
        if (userToGrant == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(UserToGrantNotFoundMessage, username),
                cancellationToken: cancellationToken);
            return;
        }
        
        if (user.Id == userToGrant.Id)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: YouAreWhoYouAre,
                cancellationToken: cancellationToken);
            return;
        }

        if (userToGrant.Role == Role.Nobody || user.Role < Role.Admin && user.Role <= userToGrant.Role)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(NotAccessibleUserRole, username),
                cancellationToken: cancellationToken);
            return;
        }
        
        if (userToGrant.Role == Role.Admin)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(TopUserRoleMessage, userToGrant.Role.ToString()),
                cancellationToken: cancellationToken);
            return;
        }
        
        userToGrant.Role += 1;
        await userRepository.SaveAsync(userToGrant, cancellationToken);

        var sendToExecutor = botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: string.Format(SuccessGrantMessage, username, userToGrant.Role.ToString()),
            cancellationToken: cancellationToken);
        var sendToGranted = botClient.SendTextMessageAsync(
            chatId: userToGrant.Id,
            text: string.Format(GrantedMessage, userToGrant.Role.ToString()),
            cancellationToken: cancellationToken);

        Task.WaitAll([sendToExecutor, sendToGranted], cancellationToken);
    }
}