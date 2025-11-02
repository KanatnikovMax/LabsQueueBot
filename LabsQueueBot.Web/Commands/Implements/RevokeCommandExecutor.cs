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

public class RevokeCommandExecutor(
    IUserRepository userRepository,
    IOptions<TelegramBotSettings> botOptions,
    IOptions<CommandsSettings> commandsOptions,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor // TODO проверить
{
    private const string EnterUsernameMessage = "Введите имя пользователя, которому хотите понизить роль";
    private const string InvalidUsernameMessage = "Введено некорректное имя пользователя:\n{0}";
    private const string UserToRevokeNotFoundMessage = "Пользователя с именем {0} не существует";
    private const string YouAreWhoYouAre = "Понизить роль самому себе нельзя. А зачем?..";
    private const string MasterAdminCantBeRevokedMessage =
        """
        Невозможно понизить роль master-администратору {0}.
        Если вы считаете, что это необходимо, обратитесь за помощью к другим master-администраторам.
        """;
    private const string NotAccessibleUserRole = "Вы не можете понизить роль пользователю {0}";
    private const string LowUserRoleMessage = "Пользователь уже имеет наименьшую роль - {0}";
    private const string SuccessRevokeMessage = "Роль пользователя {0} успешно понижена. Новая роль - {1}";
    private const string RevokedMessage = "Ваша роль понижена. Новая роль - {0}";

    public override string Type => commandsOptions.Value.Revoke.Type;
    public override string Name => commandsOptions.Value.Revoke.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.Revoke, UpdateType.Message) ];
    public override Role AcceptRole => Role.Privileged;
    public override string Definition => commandsOptions.Value.Revoke.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        switch (user.State)
        {
            case UserState.None:
            {
                await SendEnterUsername(botClient, user, cancellationToken);
                isSuccess = true;
                break;
            }
            case UserState.Revoke:
            {
                await RevokeChosenUser(botClient, update, user, cancellationToken);
                isSuccess = true;
                break;
            }
        }
        return isSuccess;
    }
    
    private async Task SendEnterUsername(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.Revoke;
        await userRepository.SaveAsync(user, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: EnterUsernameMessage,
            cancellationToken: cancellationToken);
    }

    private async Task RevokeChosenUser(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
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
        
        var userToRevoke = (await userRepository.GetByConditionAsync(u => u.Username == username, cancellationToken: cancellationToken))
            .FirstOrDefault();
        
        if (userToRevoke == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(UserToRevokeNotFoundMessage, username),
                cancellationToken: cancellationToken);
            return;
        }
        
        if (user.Id == userToRevoke.Id)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: YouAreWhoYouAre,
                cancellationToken: cancellationToken);
            return;
        }

        var isMasterAdmin = botOptions.Value.AdminChatId.Contains(user.Id);
        var isMasterAdminRevoked = botOptions.Value.AdminChatId.Contains(userToRevoke.Id);
        if (user.Role == Role.Admin && !isMasterAdmin && isMasterAdminRevoked)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(MasterAdminCantBeRevokedMessage, username),
                cancellationToken: cancellationToken);
            return;
        }
        
        if (!isMasterAdmin || user.Role != Role.Admin)
        {
            if (userToRevoke.Role == Role.Nobody || userToRevoke.Role > Role.Default && user.Role <= userToRevoke.Role)
            {
                await botClient.SendTextMessageAsync(
                    chatId: user.Id,
                    text: string.Format(NotAccessibleUserRole, username),
                    cancellationToken: cancellationToken);
                return;
            }
        
            if (userToRevoke.Role == Role.Default)
            {
                await botClient.SendTextMessageAsync(
                    chatId: user.Id,
                    text: string.Format(LowUserRoleMessage, userToRevoke.Role.ToString()),
                    cancellationToken: cancellationToken);
                return;
            }
        }  

        userToRevoke.Role -= 1;
        await userRepository.SaveAsync(userToRevoke, cancellationToken);
        
        var sendToExecutor = botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: string.Format(SuccessRevokeMessage, username, userToRevoke.Role.ToString()),
            cancellationToken: cancellationToken);
        var sendToGranted = botClient.SendTextMessageAsync(
            chatId: userToRevoke.Id,
            text: string.Format(RevokedMessage, userToRevoke.Role.ToString()),
            cancellationToken: cancellationToken);

        await Task.WhenAll(sendToExecutor, sendToGranted);
    }
}