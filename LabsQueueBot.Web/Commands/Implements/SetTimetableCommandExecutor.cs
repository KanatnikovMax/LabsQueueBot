using System.Text;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using LabsQueueBot.Web.Providers;
using LabsQueueBot.Web.Providers.Services;
using LabsQueueBot.Web.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class SetTimetableCommandExecutor(
    IQueueInfoNotificationService queueInfoNotificationService,
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    CommandsSettings commandsSettings) : ICommandExecutor // TODO доделать
{
    private readonly Dictionary<long, long> _usersChosenSubjects = new();
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string WrongCallbackQueryMessageRequest = "Не в той табличке ты тыкнул";
    private const string WrongUserAcceptRole = """
                                               У вас недостаточно прав для этого действия.
                                               Если вы так не считаете - обратитесь к администратору
                                               """;
    private const string UserDidNotСhooseMessage = "Вы не можете установить расписание, так как не выбирали дисциплину";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    private const string WrongTimetableFormatMessage = "Расписание введено в неверном формате.";
    private const string CompleteSetTimetableMessage = "Расписание для предмета установлено:";
    
    public string Type => commandsSettings.ShowTimetableCommand.Type;
    public string Name => commandsSettings.SetTimetableCommand.Name;
    public IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [
        (UserState.SetTimetable, UpdateType.Message),
        (UserState.SetTimetableDays, UpdateType.Message)
    ];
    public Role AcceptRole => Role.Privileged;
    public string Definition => commandsSettings.SetTimetableCommand.Definition;
    
    public async Task Execute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        /*
        switch (user.State)
        {
            case UserState.None:
            {
                user.State = UserState.SetTimetable;
                await userRepository.SaveAsync(user, cancellationToken);
                
                await SendSubjectsKeyboard(botClient, user, cancellationToken);
                
                return;
            }
            case UserState.SetTimetable:
            {
                if (update.Type != UpdateType.CallbackQuery)
                {
                    await botClient.DeleteMessageAsync(
                        chatId: user.Id,
                        messageId: update.Message.MessageId,
                        cancellationToken: cancellationToken);
                    return;
                }
                
                await ChooseSubject(botClient, update, user, cancellationToken);
                return;
            }
            case UserState.SetTimetableDays:
            {
                if (update.Type != UpdateType.Message || update.Message.Type != MessageType.Text)
                {
                    await botClient.DeleteMessageAsync(
                        chatId: user.Id,
                        messageId: update.Message.MessageId,
                        cancellationToken: cancellationToken);
                    return;
                }
                
                await SetTimetableDays(botClient, update, user, cancellationToken);
                return;
            }
        }
        */
    }
    /*
    private async Task SendSubjectsKeyboard(ITelegramBotClient botClient, User user, 
        CancellationToken cancellationToken)
    {
        var subjects = (await subjectsRepository.GetByConditionAsync(s =>
                    s.CourseNumber == user.CourseNumber && s.GroupNumber == user.GroupNumber,
                cancellationToken))
            .Select(s => s.SubjectName).ToList();
        var keyboard = InlineKeyboardHelper.ListToKeyboard(subjects, false, true, 1);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SendSubjectsKeyboardMessage,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task ChooseSubject(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(
            chatId: user.Id,
            messageId: update.CallbackQuery.Message.MessageId,
            cancellationToken: cancellationToken);
        
        if (user.Role < AcceptRole)
        {
            user.State = UserState.None;
            await userRepository.SaveAsync(user, cancellationToken);
            
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: WrongUserAcceptRole,
                cancellationToken: cancellationToken);
            return;
        }
        
        if (update.CallbackQuery.Message.Text != SendSubjectsKeyboardMessage)
        {
            user.State = UserState.None;
            await userRepository.SaveAsync(user, cancellationToken);
            
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: WrongCallbackQueryMessageRequest,
                cancellationToken: cancellationToken);
            return;
        }

        var subjectName = update.CallbackQuery.Data;
        
        if (subjectName == InlineKeyboardHelper.BackMessage)
        {
            user.State = UserState.None;
            await userRepository.SaveAsync(user, cancellationToken);
            return;
        }
        
        var subject = await subjectsRepository.GetByGroupAndName(user.CourseNumber, user.GroupNumber, subjectName!, cancellationToken);

        if (subject is null)
        {
            user.State = UserState.None;
            await userRepository.SaveAsync(user, cancellationToken);
            
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: SubjectNotFoundMessage,
                cancellationToken: cancellationToken);
            return;
        }

        _usersChosenSubjects[user.Id] = subject.Id;
        
        user.State = UserState.SetTimetableDays;
        await userRepository.SaveAsync(user, cancellationToken);
    }

    private async Task SetTimetableDays(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);
        
        if (user.Role < AcceptRole)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: WrongUserAcceptRole,
                cancellationToken: cancellationToken);
            return;
        }
        
        if (!_usersChosenSubjects.Remove(user.Id, out var subjectId))
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: UserDidNotСhooseMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var subject = await subjectsRepository.GetByIdAsync(subjectId, cancellationToken);
        
        if (subject is null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: SubjectNotFoundMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var rawTimetable = update.Message.Text.Split(' ');
        if (rawTimetable.Length < 2)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: WrongTimetableFormatMessage,
                cancellationToken: cancellationToken);
            return;
        }
        
        try
        {
            var week = ParseWeek(rawTimetable[0]);
            var days = rawTimetable.Skip(1).Select(ParseDayOfWeek).ToList();

            var builder = new StringBuilder();
            foreach (var day in days)
            {
                builder.Append(day);
            }
            var timetable = builder.ToString();

            await SaveSubjectTimetable(subject, week, timetable, cancellationToken);
            
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: $"{CompleteSetTimetableMessage} {subject.SubjectName}",
                cancellationToken: cancellationToken);
        }
        catch (InvalidCastException e)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: $"{WrongTimetableFormatMessage} {e.Message}",
                cancellationToken: cancellationToken);
        }
    }

    private static int ParseWeek(string week)
    {
        return week.ToLower() switch
        {
            "числитель.знаменатель" => 0,
            "числит.знаменат" => 0,
            "числ.знамен" => 0,
            "числ.знам" => 0,
            "чис.зн" => 0,
            "ч.з" => 0,
            "чз" => 0,
            "0" => 0,
            "числитель" => 1,
            "числит" => 1,
            "числ" => 1,
            "чис" => 1,
            "ч" => 1,
            "1" => 1,
            "знаменатель" => 2,
            "знаменат" => 2,
            "знамен" => 2,
            "знам" => 2,
            "зн" => 2,
            "з" => 2,
            "2" => 2,
            _ => throw new InvalidCastException($"Неправильный формат недели: {week}")
        };
    }
    
    private static int ParseDayOfWeek(string day)
    {
        return day.ToLower() switch
        {
            "понедельник"  => 1,
            "пн" => 1,
            "1" => 1,
            "вторник" => 2,
            "вт" => 2,
            "2" => 2,
            "среда" => 3,
            "ср" => 3,
            "3" => 3,
            "четверг" => 4,
            "чт" => 4,
            "4" => 4,
            "пятница" => 5,
            "пт" => 5,
            "5" => 5,
            "суббота" => 6,
            "сб" => 6,
            "6" => 6,
            "воскресенье" => 7,
            "вс" => 7,
            "7" => 7,
            _ => throw new InvalidCastException($"Не существует такого дня недели: {day}")
        };
    }

    private async Task SaveSubjectTimetable(Subject subject, int week, string timetable, CancellationToken cancellationToken)
    {
        switch (week)
        {
            case 0:
            {
                subject.DenWeekTimetableMask = timetable;
                subject.NumWeekTimetableMask = timetable;
                break;
            }
            case 1:
            {
                subject.DenWeekTimetableMask = timetable;    
                break;
            }
            case 2:
            {
                subject.NumWeekTimetableMask = timetable;
                break;
            }
        }

        await subjectsRepository.SaveAsync(subject, cancellationToken);
    }
    */
}