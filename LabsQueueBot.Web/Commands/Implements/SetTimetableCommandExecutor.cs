using System.Text.RegularExpressions;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class SetTimetableCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    IOptions<CommandsSettings> options,
    IQueueInfoNotificationService queueInfoNotificationService,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string SendChooseSubjectTimetableMessage = 
        """
        Введите название дисциплины, числитель/знаменатель и расписание. Пример:
        subject_name
        числитель/ч
        пн чт вс/1 4 7
        """;
    private const string InvalidTimetableFormatMessage = "Расписание введено в неверном формате";
    private const string InvalidWeekDaysMessage = "Некорректный формат дней недели";
    private const string SubjectNotFoundMessage = "Дисциплины {0} не существует";
    private const string CompleteSetTimetableMessage = 
        """
        Новое расписание для дисциплины {0}:
        {1}
        """;

    private readonly Regex _timetableInfoPattern = new(
        @"^(?<subject>.+?)\r?\n(?<week>(?:числитель|числ|ч|знаменатель|знам|зн|з))\n(?<days>.+?)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );
    
    public override string Type => options.Value.SetTimetable.Type;
    public override string Name => options.Value.SetTimetable.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [
        (UserState.None, UpdateType.Message),
        (UserState.SetTimetable, UpdateType.Message)
    ];
    public override Role AcceptRole => Role.Privileged;
    public override string Definition => options.Value.SetTimetable.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        switch (user.State)
        {
            case UserState.None:
            {
                await SendChooseSubjectTimetable(botClient, user, cancellationToken);
                isSuccess = true;
                break;
            }
            case UserState.SetTimetable:
            {
                await SendChooseWeekDay(botClient, update, user, cancellationToken);
                isSuccess = true;
                
                break;
            }
        }

        return isSuccess;
    }

    private async Task SendChooseSubjectTimetable(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.SetTimetable;
        await userRepository.SaveAsync(user, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SendChooseSubjectTimetableMessage,
            cancellationToken: cancellationToken);
    }

    private async Task SendChooseWeekDay(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);
        
        var timetableInfo = update.Message?.Text;
        if (timetableInfo == null || !_timetableInfoPattern.IsMatch(timetableInfo))
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: InvalidTimetableFormatMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var splitTimetableInfo = timetableInfo.Split('\n');
        var subjectName = splitTimetableInfo[0];
        var isNumWeek = splitTimetableInfo[1].ToLower().StartsWith('ч');
        var daysOfWeek = splitTimetableInfo[2].Split(' ').Select(WeekDaysHelper.Parse).Distinct().ToList();

        if (daysOfWeek.Contains(WeekDays.None))
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: InvalidWeekDaysMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var subject = await subjectsRepository.GetByGroupAndName(user.CourseNumber, user.GroupNumber, subjectName, cancellationToken);
        if (subject == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(SubjectNotFoundMessage, subjectName),
                cancellationToken: cancellationToken);
            return;
        }

        var weekDaysMask = daysOfWeek.Aggregate(WeekDays.None, (current, day) => current | day);
        
        if (isNumWeek)
            subject.NumWeekTimetableMask = (int)weekDaysMask;
        else
            subject.DenWeekTimetableMask = (int)weekDaysMask;
        await subjectsRepository.SaveAsync(subject, cancellationToken);

        var prettifiedWeekDays = daysOfWeek.Select(WeekDaysHelper.ToString).Aggregate((left, right) => $"{left} {right}");
        var notifyMessage = string.Format(CompleteSetTimetableMessage, subjectName, prettifiedWeekDays);
        await queueInfoNotificationService.NotifyGroupAboutTimetable(user.CourseNumber, user.GroupNumber, notifyMessage, cancellationToken);
    }
}