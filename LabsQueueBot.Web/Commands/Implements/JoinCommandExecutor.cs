using System.Text.Json;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Utils;
using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class JoinCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    ISerialNumberRepository serialNumberRepository,
    CommandsSettings commandsSettings,
    ILogger logger) : ICommandExecutor // TODO проверить
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    // private const string WrongCallbackQueryMessageRequest = "Не в той табличке ты тыкнул";
    private const string AddSubjectMessage = "Введите название дисциплины, которую хотите добавить";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    private const string UserAlreadyInQueueMessage = "Ты уже записан в эту очередь\nТвой номер в очереди —";
    private const string UserAlreadyInWaitingMessage = "Ты находишься в списке ожидания";
    private const string JoinCompleteMessage = "Вы добавлены в очередь ожидания по дисциплине ";
    public string Type => commandsSettings.JoinCommand.Type;
    public string Name => commandsSettings.JoinCommand.Name;
    public IReadOnlyCollection<UserState> States => [UserState.Join];
    public Role AcceptRole => Role.Default;
    public string Definition => commandsSettings.JoinCommand.Definition;
    public async Task Execute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        switch (user.State)
        {
            case UserState.None:
            {
                if (update.Type == UpdateType.Message)
                {
                    await SendSubjectsKeyboard(botClient, user, cancellationToken);
                    return;
                }
                break;
            }
            case UserState.Join:
            {
                if (update.Type == UpdateType.CallbackQuery
                    && update.CallbackQuery!.Message!.MessageId == user.LastCallbackableMessageId)
                {
                    user.LastCallbackableMessageId = null;
                    await JoinUserIntoQueue(botClient, update, user, cancellationToken);
                    return;
                }
                break;
            }
        }
        // если при UserState.None или UserState.Join получены Update не ожидаемого типа
        if (!await BotClientUtils.DeleteUpdate(botClient, user.Id, update, cancellationToken))
        {
            // в случае если получили невозможный Update (не Message и не CallbackQuery) - игнорируем его
            var updateString = JsonSerializer.Serialize(update);
            logger.Warning("Update.MessageId is null\n\n{updateString}", updateString);
        }
    }
    
    private async Task SendSubjectsKeyboard(ITelegramBotClient botClient, User user, 
        CancellationToken cancellationToken)
    {
        user.State = UserState.Join;
        await userRepository.SaveAsync(user, cancellationToken);
        
        var subjects = (await subjectsRepository.GetByConditionAsync(
                s => s.CourseNumber == user.CourseNumber && s.GroupNumber == user.GroupNumber,
                cancellationToken))
            .Select(s => s.SubjectName)
            .ToList();
        
        var keyboard = InlineKeyboardHelper.ListToKeyboard(subjects, true, true, 1);
        
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SendSubjectsKeyboardMessage,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task JoinUserIntoQueue(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        await BotClientUtils.ClearMarkupMessage(
            botClient: botClient,
            chatId: user.Id,
            messageId: update.CallbackQuery!.Message!.MessageId,
            message: $"{SendSubjectsKeyboardMessage} {update.CallbackQuery.Data}",
            cancellationToken: cancellationToken);
        
        // if (update.CallbackQuery.Message.Text != SendSubjectsKeyboardMessage)
        // {
        //     user.State = UserState.None;
        //     await userRepository.SaveAsync(user, cancellationToken);
        //     
        //     await botClient.SendTextMessageAsync(
        //         chatId: user.Id,
        //         text: WrongCallbackQueryMessageRequest,
        //         cancellationToken: cancellationToken);
        //     return;
        // }
        
        var subjectName = update.CallbackQuery.Data;
        
        if (subjectName == InlineKeyboardHelper.BackMessage)
        {
            user.State = UserState.None;
            await userRepository.SaveAsync(user, cancellationToken);
            return;
        }

        if (subjectName == InlineKeyboardHelper.AddMessage)
        {
            user.State = UserState.AddSubject;
            await userRepository.SaveAsync(user, cancellationToken);;
            
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: AddSubjectMessage,
                cancellationToken: cancellationToken);
            return; 
        }
        
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);
        
        var subject = (await subjectsRepository.GetByConditionAsync(s =>
                s.CourseNumber == user.CourseNumber
                && s.GroupNumber == user.GroupNumber
                && s.SubjectName == subjectName,
            cancellationToken
        )).FirstOrDefault();
        
        if (subject is null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: SubjectNotFoundMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var queue = serialNumberRepository.GetQueueBySubject(subject, cancellationToken);
        var waiting = serialNumberRepository.GetWaitingBySubject(subject, cancellationToken);

        var sn = (await queue).FirstOrDefault(sn => sn.TgUserIndex == user.Id);
        if (sn is not null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: $"{UserAlreadyInQueueMessage} {sn.QueueIndex + 1}",
                cancellationToken: cancellationToken);
            return;
        }

        sn = (await waiting).FirstOrDefault(sn => sn.TgUserIndex == user.Id);
        if (sn is not null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: UserAlreadyInWaitingMessage,
                cancellationToken: cancellationToken);
            return;
        }

        sn = new SerialNumber
        {
            TgUserIndex = user.Id,
            SubjectId = subject.Id,
            QueueIndex = -2
        };
        await serialNumberRepository.SaveAsync(sn, cancellationToken);
        
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: $"{JoinCompleteMessage} {subject.SubjectName}",
            cancellationToken: cancellationToken);
    }
}