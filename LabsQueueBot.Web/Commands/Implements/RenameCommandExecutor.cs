using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Validators;
using LabsQueueBot.Repository.Repository;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Commands.Implements;

public class RenameCommandExecutor(
    ILogger logger,
    IUserRepository userRepository,
    CommandsSettings commandsSettings) : ICommandExecutor
{
    private const string EnterNewNameMessage = "Введите новые Фамилию Имя";
    private const string ErrorNameValidationMessage = "Новое имя не соответствует формату:\n{0}";
    private const string RenameCompleteMessage = "Имя успешно изменено";
    public string Type => commandsSettings.RenameCommand.Type;
    public string Name => commandsSettings.RenameCommand.Name;
    public IReadOnlyCollection<UserState> States => [UserState.Rename];
    public Role AcceptRole => Role.Default;
    public string Definition => commandsSettings.RenameCommand.Definition;
    public async Task Execute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        switch (user.State)
        {
            case UserState.None:
            {
                await SendRenameMessage(botClient, user, cancellationToken);

                return;
            }
            case UserState.Rename:
            {
                if (update.Type != UpdateType.Message)
                {
                    var messageId = BotClientUpdateHelper.GetUpdateMessageId(update);
                    if (messageId is null)
                    {
                        var error = "messageId is null\n---\n" + update;
                        logger.Error(error);
                        throw new InvalidOperationException(error);
                    }

                    await botClient.DeleteMessageAsync(
                        chatId: user.Id,
                        messageId: (int)messageId,
                        cancellationToken: cancellationToken);
                    return;
                }

                await RenameUser(botClient, update, user, cancellationToken);
                
                return;
            }
            default:
            {
                return;
            }
        }
    }

    private async Task SendRenameMessage(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.Rename;
        await userRepository.SaveAsync(user, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: EnterNewNameMessage,
            cancellationToken: cancellationToken);
    }
    
    private async Task RenameUser(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);

        var name = update.Message!.Text;
        
        var validationResult = UserInfoValidator.ValidateName(name);
        if (validationResult != null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(ErrorNameValidationMessage, validationResult),
                cancellationToken: cancellationToken);
            return;
        }

        user.Name = name!;
        await userRepository.SaveAsync(user, cancellationToken);
            
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: RenameCompleteMessage,
            cancellationToken: cancellationToken);
    }
}