using LabsQueueBot.Model;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot.Controller.Commands.Appliers;

public class SetTimetableDaysApplier : Command
{
    public override string Definition => "/show_waiting_days_applier";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        var id = update.Message.Chat.Id;
        var user = Users.At(id);
        
        var group = Groups.groups[new GroupKey(user.CourseNumber, user.GroupNumber)];
        var subject = group.SubjectsToSetTimetable[id];
        group.SubjectsToSetTimetable.Remove(id);
        user.State = User.UserState.None;

        const string invalidSubjectMessage = "Такого предмета не существует";
        if (!group.ContainsKey(subject))
            return new SendMessageRequest(id, invalidSubjectMessage);

        string message;
        var rawDays = update.Message.Text.Split(' ');
        try
        {
            var days = rawDays.Select(ParseDayOfWeek).Distinct().ToList();
            group.Timetable[subject] = days;
            message = $"Добавлено новое расписание для предмета {subject}";
        }
        catch (InvalidCastException e)
        {
            message = e.Message;
        }
        catch (KeyNotFoundException)
        {
            message = invalidSubjectMessage;
        }

        return new SendMessageRequest(id, message);
    }
    
    private static DayOfWeek ParseDayOfWeek(string day)
    {
        return day.ToLower() switch
        {
            "понедельник"  => DayOfWeek.Monday,
            "пн" => DayOfWeek.Monday,
            "вторник" => DayOfWeek.Tuesday,
            "вт" => DayOfWeek.Tuesday,
            "среда" => DayOfWeek.Wednesday,
            "ср" => DayOfWeek.Wednesday,
            "четверг" => DayOfWeek.Thursday,
            "чт" => DayOfWeek.Thursday,
            "пятница" => DayOfWeek.Friday,
            "пт" => DayOfWeek.Friday,
            "суббота" => DayOfWeek.Saturday,
            "сб" => DayOfWeek.Saturday,
            "воскресенье" => DayOfWeek.Sunday,
            "вс" => DayOfWeek.Sunday,
            _ => throw new InvalidCastException($"Не существует такого дня недели: {day}")
        };
    }
}