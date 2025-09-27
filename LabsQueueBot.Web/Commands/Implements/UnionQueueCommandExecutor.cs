using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Utils;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using LabsQueueBot.Web.Services;
using Microsoft.Extensions.Options;
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
    IQueueInfoNotificationService queueInfoNotificationService,
    IOptions<CommansSettings> options,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string WrongUserAcceptRole = """
                                               У вас недостаточно прав для этого действия.
                                               Если вы так не считаете - обратитесь к администратору
                                               """;
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    private const string WaitingListIsEmpty = "Список ожидания по выбранному предмету пуст";
    private const string UnionCompleteMessage = "Очередь по выбранному предмету сформирована:";
    
    public override string Type => options.Value.Union.Type;
    public override string Name => options.Value.Union.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.Union, UpdateType.CallbackQuery) ];
    public override Role AcceptRole => Role.Privileged;
    public override string Definition => options.Value.Union.Definition;

    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        var isSuccess = false;
        switch (user.State)
        {
            case UserState.None:
            {
                await SendSubjectsKeyboard(botClient, user, cancellationToken);
                isSuccess = true;
                break;
            }
            case UserState.Union:
            {
                await UnionQueue(botClient, update, user, cancellationToken);
                isSuccess = true;
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
        
        var keyboard = InlineKeyboardHelper.ListToKeyboard(subjects, 1);

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

        await queueInfoNotificationService.NotifyGroup(user.CourseNumber, user.GroupNumber, cancellationToken);
    }
}