using System.Text.Json;
using LabsQueueBot.Core.Constants;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Extensions;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Utils;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Exceptions;
using LabsQueueBot.Web.Providers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using File = System.IO.File;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.UpdateHandler;

public class QueueBotUpdateHandler(
    ILogger logger,
    IAdminNotificationProvider adminNotificationProvider,
    IServiceScopeFactory scopeFactory,
    CommandsSettings commandsSettings) : IUpdateHandler
{
    private const string NotRegisteredMessage = """
                                                Вы не зарегистрированы!
                                                {0} для регистрации
                                                """;
    private const string InvalidUpdateMessage = "Введи команду, ящур";
    private const string AdminErrorMessage = "Что-то упало: {0}";
    private const string AdminErrorDocumentPattern = """
                                             Type: {0}
                                             
                                             Message: {1}
                                             
                                             StackTrace: {2}
                                             
                                             Last update: {3}
                                             """;

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            // проверяем: is text message | is valid callback | is left chat member
            if (!update.IsValid())
            {
                // удаляем сообщение
                // или игнорируем, если пришел невалидный UpdateType.MyChatMember
                if (update.Type != UpdateType.MyChatMember)
                {
                    await botClient.DeleteMessageAsync(
                        chatId: update.GetChatId()!,
                        messageId: update.GetMessageId()!.Value,
                        cancellationToken: cancellationToken);
                }
                return;
            }
        
            await using var scope = scopeFactory.CreateAsyncScope();
        
            var usersRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            
            var chatId = update.GetChatId()!;
            
            var user = await usersRepository.GetByIdAsync(chatId.Value, cancellationToken);

            // проверка на first message (null если да)
            if (user == null)
            {
                user = new User(chatId.Value)
                {
                    State = UserState.Unregistered
                };
                await usersRepository.SaveAsync(user, cancellationToken);
                
                // проверка на first message /start
                if (!update.IsTextMessage(commandsSettings.StartCommand.Name))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: string.Format(NotRegisteredMessage, commandsSettings.StartCommand.Name),
                        cancellationToken: cancellationToken);
                    return;
                }
            }
                    
            var commandExecutorProvider = scope.ServiceProvider.GetRequiredService<ICommandExecutorProvider>();
            
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    await HandleMessageUpdate(botClient, update, user, commandExecutorProvider, cancellationToken);
                    break;
                }
                case UpdateType.CallbackQuery:
                {
                    await HandleCallbackQueryUpdate(botClient, update, user, commandExecutorProvider, cancellationToken);
                    break;
                }
                case UpdateType.MyChatMember:
                {
                    await HandleMyChatMemberUpdate(update, user, usersRepository, cancellationToken);
                    break;
                }
            }
        }
        catch (Exception exception)
        {
            throw new CommandExecutionException(update, exception);
        }
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string errorDocumentBody;
        int documentId;
        string errorMessage;
        
        if (exception is CommandExecutionException e)
        {
            var lastUpdateBody = JsonSerializer.Serialize(e.LastUpdate);
            
            logger.Error("Ошибка выполнения команды");
            logger.Error(e.ThrownException, e.ThrownException.Message);
            logger.Error("\n");
            logger.Error("Последний update");
            logger.Error(lastUpdateBody);
            
            errorDocumentBody = string.Format(
                AdminErrorDocumentPattern,
                e.ThrownException,
                e.ThrownException.Message,
                e.ThrownException.StackTrace,
                lastUpdateBody);
            documentId = e.ThrownException.GetHashCode();
            errorMessage = string.Format(AdminErrorMessage, nameof(logger.Error));
        }
        else
        {
            logger.Fatal(exception, exception.Message);

            errorDocumentBody = JsonSerializer.Serialize(exception);
            documentId = exception.GetHashCode();
            errorMessage = string.Format(AdminErrorMessage, nameof(logger.Fatal));
        }
        
        var path = string.Format(GlobalConstants.ErrorDocumentPath, documentId);
        await File.WriteAllTextAsync(path, errorDocumentBody, cancellationToken);
            
        await adminNotificationProvider.NotifyAdminsWithDocument(documentId, errorMessage, cancellationToken);
    }

    private async Task HandleMessageUpdate(ITelegramBotClient botClient, Update update, User user,
        ICommandExecutorProvider commandExecutorProvider, CancellationToken cancellationToken)
    {
        if (!update.IsValidMessage() || user.LastCallbackableMessageId != null)
        {
            await ReactInvalidUpdate(botClient, update, user.Id, cancellationToken);
            return;
        }

        var isCommandMessage = user.State == UserState.None && update.IsCommand();

        var command = isCommandMessage
            ? commandExecutorProvider.GetCommandExecutorByText(update.Message!.Text!, user.Role)
            : commandExecutorProvider.GetCommandExecutorByState(user.State, update.Type, user.Role);
        if (command == null)
        {
            if (isCommandMessage)
            {
                await botClient.SendTextMessageAsync(
                    chatId: user.Id,
                    text: InvalidUpdateMessage,
                    cancellationToken: cancellationToken);
            }
            return;
        }

        await command.Execute(botClient, update, user, cancellationToken);
    }

    private async Task HandleCallbackQueryUpdate(ITelegramBotClient botClient, Update update, User user,
        ICommandExecutorProvider commandExecutorProvider, CancellationToken cancellationToken)
    {
        // проверка ответа на нужный Callback
        // проверка валидности если пользователь ткнул в InlineKeyboard
        if (!update.IsValidCallbackQuery(user.LastCallbackableMessageId))
        {
            await ReactInvalidUpdate(botClient, update, user.Id, cancellationToken);
            return;
        }
        
        var command = commandExecutorProvider.GetCommandExecutorByState(user.State, update.Type, user.Role);
        if (command == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: InvalidUpdateMessage, // TODO придумать другой комментарий на ошибочный callback
                cancellationToken: cancellationToken);
            return;
        }
        
        await command.Execute(botClient, update, user, cancellationToken);
    }

    private async Task HandleMyChatMemberUpdate(Update update, User user, IUserRepository userRepository, CancellationToken cancellationToken)
    {
        if (!update.IsValidMyChatMember())
            return;
        
        await userRepository.DeleteAsync(user, cancellationToken);
    }

    private async Task ReactInvalidUpdate(ITelegramBotClient botClient, Update update, long chatId, CancellationToken cancellationToken)
    {
        if (await BotClientUtils.DeleteUpdate(botClient, chatId, update, cancellationToken))
            return;
                
        // в случае если получили невозможный Update (не Message и не CallbackQuery) - игнорируем его
        var updateString = JsonSerializer.Serialize(update);
        logger.Warning("Update.MessageId is null\n\n{updateString}", updateString);
    }
}