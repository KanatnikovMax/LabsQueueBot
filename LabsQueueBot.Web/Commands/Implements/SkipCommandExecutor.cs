using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Utils;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using LabsQueueBot.Web.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class SkipCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    IQueueInfoNotificationService queueInfoNotificationService,
    CommandsSettings commandsSettings,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor // TODO: протестировано, перед деплоем надо раскомментировать рассылку
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    private const string UserIsWaitingMessage = "Ты в списке ожидания, так чего не ждётся?";
    private const string UserNotExistsInQueueMessage = "Вас нет в очереди по дисциплине ";
    private const string UserIsLastInQueue = "Ты уже итак в конце очереди, ожидай своего часа :)";
    private const string SkipCompleteMessage = "Это как шаг вперед, но назад";
    
    public override string Type => commandsSettings.SkipCommand.Type;
    public override string Name => commandsSettings.SkipCommand.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.Skip, UpdateType.CallbackQuery) ];
    public override Role AcceptRole => Role.Default;
    public override string Definition => commandsSettings.SkipCommand.Definition;
    
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
            case UserState.Skip:
            {
                if (update.Type == UpdateType.CallbackQuery
                    && update.CallbackQuery!.Message!.MessageId == user.LastCallbackableMessageId)
                {
                    user.LastCallbackableMessageId = null;
                    await SkipUserInQueue(botClient, update, user, cancellationToken);
                    isSuccess = true;
                }
                break;
            }
        }
        
        return isSuccess;
    }
    
    private async Task SendSubjectsKeyboard(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var subjects = (await subjectsRepository.GetByConditionAsync(s =>
                    s.CourseNumber == user.CourseNumber && s.GroupNumber == user.GroupNumber,
                cancellationToken))
            .Select(s => s.SubjectName).ToList();
        var keyboard = InlineKeyboardHelper.ListToKeyboard(subjects, false, true, 1);

        var message = await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SendSubjectsKeyboardMessage,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
        
        user.State = UserState.Skip;
        user.LastCallbackableMessageId = message.MessageId;
        await userRepository.SaveAsync(user, cancellationToken);
    }

    private async Task SkipUserInQueue(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        await BotClientUtils.ClearMarkupMessage(
            botClient: botClient,
            chatId: user.Id,
            messageId: update.CallbackQuery!.Message!.MessageId,
            message: $"{SendSubjectsKeyboardMessage} {update.CallbackQuery.Data}",
            cancellationToken: cancellationToken);

        var subjectName = update.CallbackQuery!.Data;

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
        
        if (subject.Waiting.Contains(user.Id))
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: UserIsWaitingMessage,
                cancellationToken: cancellationToken);
            return;
        }
        
        var queueIndex = subject.Queue.ToList().IndexOf(user.Id);
        if (queueIndex == -1)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: UserNotExistsInQueueMessage,
                cancellationToken: cancellationToken);
            return;
        }

        if (queueIndex == subject.Queue.Length - 1)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: UserIsLastInQueue,
                cancellationToken: cancellationToken);
            return;
        }

        var skippedUserId = subject.Queue[queueIndex + 1];

        (subject.Queue[queueIndex], subject.Queue[queueIndex + 1]) = (subject.Queue[queueIndex + 1], subject.Queue[queueIndex]);
        await subjectsRepository.SaveAsync(subject, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SkipCompleteMessage,
            cancellationToken: cancellationToken);

        // await queueInfoNotificationService.NotifyUserBySubject(skippedUserId, subject.SubjectName, cancellationToken);
    }
}