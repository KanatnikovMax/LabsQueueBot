using System.Text.Json;
using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Utils;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using LabsQueueBot.Web.Providers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class UnionQueueCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    IRandomizeUnionWaitingService randomizeUnionWaitingService,
    IQueueInfoNotificationProvider queueInfoNotificationProvider,
    CommandsSettings commandsSettings,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string WrongUserAcceptRole = """
                                               У вас недостаточно прав для этого действия.
                                               Если вы так не считаете - обратитесь к администратору
                                               """;
    // private const string WrongCallbackQueryMessageRequest = "Не в той табличке ты тыкнул";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    private const string WaitingListIsEmpty = "Список ожидания по выбранному предмету пуст";
    private const string UnionCompleteMessage = "Очередь по выбранному предмету сформирована:";
    public override string Type => commandsSettings.UnionQueueCommand.Type;
    public override string Name => commandsSettings.UnionQueueCommand.Name;
    public override IReadOnlyCollection<UserState> States => [UserState.Union];
    public override Role AcceptRole => Role.Privileged;
    public override string Definition => commandsSettings.UnionQueueCommand.Definition;

    protected override async Task InternalExecute(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
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
            case UserState.Union:
            {
                if (update.Type == UpdateType.CallbackQuery)
                {
                    await UnionQueue(botClient, update, user, cancellationToken);
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
        
        user.State = UserState.Union;
        user.LastCallbackableMessageId = message.MessageId;
        await userRepository.SaveAsync(user, cancellationToken);
    }

    private async Task UnionQueue(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        await BotClientUtils.ClearMarkupMessage(
            botClient: botClient,
            chatId: user.Id,
            messageId: update.CallbackQuery!.Message!.MessageId,
            message: $"{SendSubjectsKeyboardMessage} {update.CallbackQuery.Data}",
            cancellationToken: cancellationToken);

        // по идее никогда не выполняется
        if (user.Role < AcceptRole)
        {
            user.State = UserState.None;
            await userRepository.SaveAsync(user, cancellationToken);

            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: WrongUserAcceptRole,
                cancellationToken: cancellationToken);
        }

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
        
        if (subject.Waiting.Length == 0)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: WaitingListIsEmpty,
                cancellationToken: cancellationToken);
            return;
        }

        await randomizeUnionWaitingService.RandomizeAndUnionWaitingBySubject(subject.Id, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: $"{UnionCompleteMessage} {subject.SubjectName}",
            cancellationToken: cancellationToken);

        await queueInfoNotificationProvider.NotifyGroup(user.CourseNumber, user.GroupNumber, cancellationToken);
    }
}