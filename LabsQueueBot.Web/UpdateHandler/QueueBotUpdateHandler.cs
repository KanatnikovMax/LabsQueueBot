using LabsQueueBot.Core.Constants;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Helpers;
using Newtonsoft.Json;
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
    INotificationProvider notificationProvider,
    IServiceScopeFactory scopeFactory) : IUpdateHandler
{
    private const string WrongCommandRequestMessage = "Введи команду, ящур";
    private const string AdminErrorMessage = "Что-то упало, уровень: {0}";

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            // check non text messages
            if (update.Type == UpdateType.Message && update.Message!.Type != MessageType.Text)
            {
                await botClient.DeleteMessageAsync(
                    chatId: update.Message.Chat.Id,
                    messageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);
                return;
            }
        
            await using var scope = scopeFactory.CreateAsyncScope();
        
            var usersRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
            // TODO проверить first message
            var id = update.Message?.Chat.Id 
                     ?? update.CallbackQuery?.Message?.Chat.Id
                     ?? update.MyChatMember?.Chat.Id;
            var user = await usersRepository.GetByIdAsync(id!.Value, cancellationToken);
            if (user is null && update.Type == UpdateType.Message && update.Message?.Text != "/start")
            {
                await botClient.SendTextMessageAsync(
                    chatId: id,
                    text: "Вы не зарегистрированы!\n/start для регистрации",
                    cancellationToken: cancellationToken);
                return;
            }

            var commandProvider = scope.ServiceProvider.GetRequiredService<ICommandProvider>();
        
            if (update.Type == UpdateType.MyChatMember)
            {
                await HandleMyChatMember(update, usersRepository, cancellationToken);
            }
            else
            {
                await HandleDefaultUpdate(botClient, update, commandProvider, usersRepository, cancellationToken);
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
            var exceptionBody = JsonConvert.SerializeObject(e.ThrownException);
            var lastUpdateBody = JsonConvert.SerializeObject(e.LastUpdate);
            
            logger.Error("Ошибка выполнения команды");
            logger.Error(e.ThrownException, e.ThrownException.Message);
            logger.Error("\n");
            logger.Error(exceptionBody);
            logger.Error("\n");
            logger.Error("Последний update");
            logger.Error(lastUpdateBody);

            errorDocumentBody = $"{exceptionBody}\n\n\n{lastUpdateBody}";
            documentId = e.ThrownException.GetHashCode();
            errorMessage = string.Format(AdminErrorMessage, nameof(logger.Error));
        }
        else
        {
            logger.Fatal(exception, exception.Message);

            errorDocumentBody = JsonConvert.SerializeObject(exception);
            documentId = exception.GetHashCode();
            errorMessage = string.Format(AdminErrorMessage, nameof(logger.Fatal));
        }
        
        var path = string.Format(GlobalConstants.ErrorDocumentPath, documentId);
        await File.WriteAllTextAsync(path, errorDocumentBody, cancellationToken);
            
        await notificationProvider.NotifyAdminsWithDocument(documentId, errorMessage, cancellationToken);
    }

    private async Task InternalHandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // check non text messages
        if (update.Type == UpdateType.Message && update.Message.Type != MessageType.Text)
        {
            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.MessageId,
                cancellationToken: cancellationToken);
            return;
        }
        
        await using var scope = scopeFactory.CreateAsyncScope();
        
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
        // TODO проверить first message
        var id = update.Message?.Chat.Id 
                 ?? update.CallbackQuery?.Message?.Chat.Id
                 ?? update.MyChatMember?.Chat.Id;
        if (id == null)
            return;
        
        var user = await usersRepository.GetByIdAsync(id.Value, cancellationToken);
        if (user is null && update.Type == UpdateType.Message && update.Message?.Text != "/start")
        {
            await botClient.SendTextMessageAsync(
                chatId: id,
                text: "Вы не зарегистрированы!\n/start для регистрации",
                cancellationToken: cancellationToken);
            return;
        }

        var commandProvider = scope.ServiceProvider.GetRequiredService<ICommandProvider>();
        
        if (update.Type == UpdateType.MyChatMember)
        {
            await HandleMyChatMember(update, usersRepository, cancellationToken);
        }
        else
        {
            await HandleDefaultUpdate(botClient, update, commandProvider, usersRepository, cancellationToken);
        }
    }

    private async Task HandleDefaultUpdate(ITelegramBotClient botClient, Update update,
        ICommandProvider commandProvider, IUserRepository usersRepository, CancellationToken cancellationToken)
    {
        var id = BotClientUpdateHelper.GetUpdateChatId(update);
        // var id = update.Type == UpdateType.Message ? update.Message.Chat.Id : update.CallbackQuery.Message.Chat.Id;
        var user = await usersRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            user = new User(id)
            {
                State = UserState.Unregistered
            };
            await usersRepository.SaveAsync(user, cancellationToken);
        }

        var command = user.State == UserState.None
            ? commandProvider.GetCommandByText(update.Message.Text, user.Role)
            : commandProvider.GetCommandByState(user.State, user.Role);
        
        if (command is not null)
        {
            await command.Execute(botClient, update, user, cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: WrongCommandRequestMessage,
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleMyChatMember(Update update, IUserRepository usersRepository, CancellationToken cancellationToken)
    {
        var id = update.MyChatMember.Chat.Id;
        var user = await usersRepository.GetByIdAsync(id, cancellationToken);
        await usersRepository.DeleteAsync(user, cancellationToken);
    }
}