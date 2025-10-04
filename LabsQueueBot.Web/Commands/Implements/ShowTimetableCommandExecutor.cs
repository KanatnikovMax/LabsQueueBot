using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Utils;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class ShowTimetableCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    IOptions<CommandsSettings> options,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    private const string SubjectTimetableMessage =
        """
        Расписание для дисциплины {0}:
        Числитель: {1}
        Знаменатель: {2}
        """;
    
    public override string Type => options.Value.ShowTimetable.Type;
    public override string Name => options.Value.ShowTimetable.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.ShowTimetable, UpdateType.CallbackQuery) ];
    public override Role AcceptRole => Role.Default;
    public override string Definition => options.Value.ShowTimetable.Definition;

    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
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
            case UserState.ShowTimetable:
            {
                user.LastCallbackableMessageId = null;
                await SendSubjectTimetable(botClient, update, user, cancellationToken);
                isSuccess = true;
                break;
            }
        }
        return isSuccess;
    }
    
    private async Task SendSubjectsKeyboard(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken)
    {
        var subjects = (await subjectsRepository.GetByConditionAsync(s =>
                    s.CourseNumber == user.CourseNumber && s.GroupNumber == user.GroupNumber,
                cancellationToken))
            .Select(s => s.SubjectName).ToList();
        var keyboard = InlineKeyboardHelper.ListToKeyboard(subjects, 1);

        var message = await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SendSubjectsKeyboardMessage,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
        
        user.State = UserState.ShowTimetable;
        user.LastCallbackableMessageId = message.MessageId;
        await userRepository.SaveAsync(user, cancellationToken);
    }

    private async Task SendSubjectTimetable(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        await BotClientUtils.ClearMarkupMessage(
            botClient: botClient,
            chatId: user.Id,
            messageId: update.CallbackQuery!.Message!.MessageId,
            message: $"{SendSubjectsKeyboardMessage} {update.CallbackQuery.Data}",
            cancellationToken: cancellationToken);
        
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);
        
        var subjectName = update.CallbackQuery.Data;
        
        if (subjectName == InlineKeyboardHelper.BackMessage)
        {
            return;
        }

        var subject = await subjectsRepository.GetByGroupAndName(user.CourseNumber, user.GroupNumber, subjectName!, cancellationToken);
        
        if (subject is null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: SubjectNotFoundMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var numWeekTimetable = WeekDaysHelper.ParseWeekDays((WeekDays)subject.NumWeekTimetableMask);
        var denWeekTimetable = WeekDaysHelper.ParseWeekDays((WeekDays)subject.DenWeekTimetableMask);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: string.Format(SubjectTimetableMessage, subjectName, WeekDaysHelper.ToString(numWeekTimetable), WeekDaysHelper.ToString(denWeekTimetable)),
            cancellationToken: cancellationToken);
    }
}