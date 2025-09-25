using System.Text.Json;
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

public class QuitCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    CommandsSettings commandsSettings,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    private const string QuitQueueCompleteMessage = "Вы вышли из очереди по дисциплине {0}";
    private const string QuitWaitingCompleteMessage = "Вы вышли из списка ожидания по дисциплине {0}";
    private const string UserNotExistsInQueueMessage = "Вас нет в очереди по дисциплине {0}";
    
    public override string Type => commandsSettings.QuitCommand.Type;
    public override string Name => commandsSettings.QuitCommand.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.Quit, UpdateType.CallbackQuery) ];
    public override Role AcceptRole => Role.Default;
    public override string Definition => commandsSettings.QuitCommand.Definition;

    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
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
            case UserState.Quit:
            {
                if (update.Type == UpdateType.CallbackQuery
                    && update.CallbackQuery!.Message!.MessageId == user.LastCallbackableMessageId)
                {
                    user.LastCallbackableMessageId = null;
                    await QuitUserFromQueue(botClient, update, user, cancellationToken);
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
        
        var keyboard = InlineKeyboardHelper.ListToKeyboard(subjects, false, true, 1);

        var message = await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SendSubjectsKeyboardMessage,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
        
        user.State = UserState.Quit;
        user.LastCallbackableMessageId = message.MessageId;
        await userRepository.SaveAsync(user, cancellationToken);
    }

    private async Task QuitUserFromQueue(ITelegramBotClient botClient, Update update, User user,
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

        if (subject.Queue.Contains(user.Id))
        {
            subject.Queue = subject.Queue.Where(x => x != user.Id).ToArray();
            await subjectsRepository.SaveAsync(subject, cancellationToken);
            
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(QuitQueueCompleteMessage, subject.SubjectName),
                cancellationToken: cancellationToken);
            return;
        }
        
        if (subject.Waiting.Contains(user.Id))
        {
            subject.Waiting = subject.Waiting.Where(x => x != user.Id).ToArray();
            await subjectsRepository.SaveAsync(subject, cancellationToken);
            
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(QuitWaitingCompleteMessage, subject.SubjectName),
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: string.Format(UserNotExistsInQueueMessage, subject.SubjectName),
            cancellationToken: cancellationToken);
    }
}