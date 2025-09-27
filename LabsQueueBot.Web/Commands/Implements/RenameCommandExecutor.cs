using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Extensions;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Validators;
using LabsQueueBot.Repository.Repository;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Commands.Implements;

public class RenameCommandExecutor(
    IUserRepository userRepository,
    IOptions<CommandsSettings> options,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string EnterNewNameMessage = "Введите новые Фамилию Имя";
    private const string ErrorNameValidationMessage = "Новое имя не соответствует формату:\n{0}";
    private const string RenameCompleteMessage = "Имя успешно изменено";
    
    public override string Type => options.Value.Rename.Type;
    public override string Name => options.Value.Rename.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.Rename, UpdateType.Message) ];
    public override Role AcceptRole => Role.Default;
    public override string Definition => options.Value.Rename.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        switch (user.State)
        {
            case UserState.None:
            {
                await SendRenameMessage(botClient, user, cancellationToken);
                isSuccess = true;
                break;
            }
            case UserState.Rename:
            {
                if (update.Type != UpdateType.Message)
                {
                    var messageId = update.GetMessageId();
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
                    isSuccess = false;
                    break;
                }

                await RenameUser(botClient, update, user, cancellationToken);
                isSuccess = true;
                
                break;
            }
        }
        
        return isSuccess;
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