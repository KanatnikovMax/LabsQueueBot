using System.Text.Json;
using LabsQueueBot.Core.Constants;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Extensions;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Utils;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Models;
using LabsQueueBot.Web.Providers;
using LabsQueueBot.Web.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using File = System.IO.File;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.UpdateHandler;

public class QueueBotUpdateHandler(
    IServiceScopeFactory scopeFactory,
    IAdminNotificationService adminNotificationService,
    IOptions<CommandsSettings> options,
    ILogger logger) : IUpdateHandler
{
    private const string UnregisteredMessage =
        """
        Вы не зарегистрированы!
        {0} для регистрации
        """;
    private const string InvalidMessageUpdateMessage = "Введи команду, ящур";
    private const string FailedUpdateMessage =
        """
        Что-то пошло не так =(
        Пожалуйста, сообщи администратору о сбое
        """;
    private const string AdminErrorMessage = "Что-то не сработало! {0}";
    private const string AdminFatalMessage = "Ботяра упал =(";
    
    private readonly JsonSerializerOptions _errorSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        //// славянский ретерн в мейне
        // return;
            
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
        
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            
            var usersRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            
            var chatId = update.GetChatId()!;
            
            var user = await usersRepository.GetByIdAsync(chatId.Value, cancellationToken);

            // проверка на first message (null если да)
            if (user == null)
            {
                user = new User
                {
                    Id = chatId.Value,
                    Role = Role.Nobody,
                    State = UserState.Unregistered
                };
                await usersRepository.SaveAsync(user, cancellationToken);
                
                // проверка на first message /start
                if (!update.IsTextMessage(options.Value.Start.Name))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: string.Format(UnregisteredMessage, options.Value.Start.Name),
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
                    await HandleMyChatMemberUpdate(botClient, update, user, usersRepository, cancellationToken);
                    break;
                }
            }
        }
        catch (Exception exception)
        {
            var sendFailedUpdateResponse = botClient.SendTextMessageAsync(
                chatId: update.GetChatId()!,
                text: FailedUpdateMessage,
                cancellationToken: cancellationToken);
            
            var notifyAdmins = InternalHandleException(
                thrownException: exception,
                lastUpdate: update,
                cancellationToken: cancellationToken);

            await Task.WhenAll(notifyAdmins, sendFailedUpdateResponse);
        }
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        await InternalHandleException(
            thrownException: exception,
            cancellationToken: cancellationToken);
    }

    private async Task HandleMessageUpdate(ITelegramBotClient botClient, Update update, User user,
        ICommandExecutorProvider commandExecutorProvider, CancellationToken cancellationToken)
    {
        // проверка, на валидность сообщения и что пользователь в данный момент не должен ответить на InlineKeyboard
        if (!update.IsValidMessage() || user.LastCallbackableMessageId != null)
        {
            await ReactAnInvalidUpdate(botClient, update, user.Id, cancellationToken);
            return;
        }
        
        var command = user.State == UserState.None && update.IsCommand()
            ? commandExecutorProvider.GetCommandExecutorByText(update.Message!.Text!, user.Role)
            : commandExecutorProvider.GetCommandExecutorByState(user.State, update.Type, user.Role);
        if (command == null)
        {
            if (user.State == UserState.None)
            {
                await botClient.SendTextMessageAsync(
                    chatId: user.Id,
                    text: InvalidMessageUpdateMessage,
                    cancellationToken: cancellationToken);
            }
            return;
        }
        
        // проверка на Username (для обновления данных пользователей)
        if (user.Username == null || user.Username != update.GetUsername())
        {
            user.Username = update.GetUsername();
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
            await ReactAnInvalidUpdate(botClient, update, user.Id, cancellationToken);
            return;
        }
        
        var command = commandExecutorProvider.GetCommandExecutorByState(user.State, update.Type, user.Role);
        if (command == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: FailedUpdateMessage,
                cancellationToken: cancellationToken);
            return;
        }
        
        await command.Execute(botClient, update, user, cancellationToken);
    }

    private async Task HandleMyChatMemberUpdate(ITelegramBotClient botClient, Update update, User user,
        IUserRepository userRepository, CancellationToken cancellationToken)
    {
        if (!update.IsValidMyChatMember())
        {
            await ReactAnInvalidUpdate(botClient, update, user.Id, cancellationToken);
            return;
        }
        
        await userRepository.DeleteAsync(user, cancellationToken);
    }

    private async Task ReactAnInvalidUpdate(ITelegramBotClient botClient, Update update, long chatId, CancellationToken cancellationToken)
    {
        if (await BotClientUtils.DeleteUpdate(botClient, chatId, update, cancellationToken))
            return;
                
        // в случае если получили невозможный Update (не Message и не CallbackQuery) - игнорируем его
        var updateString = JsonSerializer.Serialize(update);
        logger.Warning("Update.MessageId is null\n\n{updateString}", updateString);
    }

    private async Task InternalHandleException(Exception thrownException, CancellationToken cancellationToken, Update? lastUpdate = null)
    {
        string errorMessage;
        
        var documentId = thrownException.GetHashCode();
        var errorReport = new ErrorReportDto
        {
            ThrownException = thrownException.GetType().FullName,
            Message = thrownException.Message,
            StackTrace = thrownException.StackTrace?.Replace(" at ", "===at ").Replace(" in ", "===in ").Split("===").Skip(1),
            LastUpdate = lastUpdate
        };
        
        if (lastUpdate == null)
        {
            errorMessage = string.Format(AdminFatalMessage);
            
            logger.Fatal(thrownException, thrownException.Message);
        }
        else
        {
            errorMessage = string.Format(AdminErrorMessage, nameof(logger.Error));
            
            var lastUpdateBody = JsonSerializer.Serialize(lastUpdate);
            logger.Error(thrownException, thrownException.Message);
            logger.Error(lastUpdateBody);
        }
        
        var errorReportDocumentBody = JsonSerializer.Serialize(errorReport, _errorSerializerOptions);
        var path = string.Format(GlobalConstants.ErrorDocumentPath, documentId);
        await File.WriteAllTextAsync(path, errorReportDocumentBody, cancellationToken);
            
        await adminNotificationService.NotifyWithDocument(documentId, errorMessage, cancellationToken);
    }
}