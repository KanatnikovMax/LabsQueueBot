using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Utils;
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
    CommandsSettings commandsSettings,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string AddSubjectMessage = "Введите название дисциплины, которую хотите добавить";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    private const string UserAlreadyInQueueMessage = "Ты уже находишься в этой очереди. Твоё место в очереди: {0}";
    private const string UserAlreadyInWaitingMessage = "Ты уже находишься в списке ожидания";
    private const string JoinCompleteMessage = "Вы добавлены в список ожидания по дисциплине {0}";
    
    public override string Type => commandsSettings.JoinCommand.Type;
    public override string Name => commandsSettings.JoinCommand.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.Join, UpdateType.CallbackQuery) ];
    public override Role AcceptRole => Role.Default;
    public override string Definition => commandsSettings.JoinCommand.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        switch (user.State)
        {
            case UserState.None:
            {
                if (update.Type == UpdateType.Message)
                {
                    await SendSubjectsKeyboard(botClient, user, cancellationToken);
                    isSuccess = true;
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
                    isSuccess = true;
                }
                break;
            }
        }

        return isSuccess;
    }
    
    private async Task SendSubjectsKeyboard(ITelegramBotClient botClient, User user, 
        CancellationToken cancellationToken)
    {
        var subjects = (await subjectsRepository.GetByConditionAsync(
                s => s.CourseNumber == user.CourseNumber && s.GroupNumber == user.GroupNumber,
                cancellationToken))
            .Select(s => s.SubjectName)
            .ToList();
        
        var keyboard = InlineKeyboardHelper.ListToKeyboard(subjects, 1, true);
        
        var message = await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SendSubjectsKeyboardMessage,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
        
        user.State = UserState.Join;
        user.LastCallbackableMessageId = message.MessageId;
        await userRepository.SaveAsync(user, cancellationToken);
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
            await userRepository.SaveAsync(user, cancellationToken);
            
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: AddSubjectMessage,
                cancellationToken: cancellationToken);
            return; 
        }
        
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);
        
        var subject = await subjectsRepository.GetByGroupAndName(user.CourseNumber, user.GroupNumber, subjectName!, cancellationToken);
        
        if (subject == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: SubjectNotFoundMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var userQueueIndex = subject.Queue
            .ToList()
            .IndexOf(user.Id);
        if (userQueueIndex != -1)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(UserAlreadyInQueueMessage, userQueueIndex + 1),
                cancellationToken: cancellationToken);
            return;
        }
        
        var userWaitingIndex = subject.Waiting
            .ToList()
            .IndexOf(user.Id);
        if (userWaitingIndex != -1)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: UserAlreadyInWaitingMessage,
                cancellationToken: cancellationToken);
            return;
        }

        subject.Waiting = subject.Waiting.Append(user.Id).ToArray();
        await subjectsRepository.SaveAsync(subject, cancellationToken);
        
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: string.Format(JoinCompleteMessage, subject.SubjectName),
            cancellationToken: cancellationToken);
    }
}