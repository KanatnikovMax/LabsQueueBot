using System.Text;
using LabsQueueBot.Model;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static System.String;

namespace LabsQueueBot.Controller.Commands.Responders;

public class ShowTimetable : Command
{
    public override string Definition => "/timetable - Показать текущее расписание для "
                                         + "формирования очереди по выбранному предмету";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }
    
    public override SendMessageRequest Run(Update update)
    {
        var id = update.Message.Chat.Id;
        var user = Users.At(update.Message.Chat.Id);
        
        var builder = new StringBuilder();
        
        builder.AppendLine("Расписание формирования очередей");

        var group = Groups.groups[new GroupKey(user.CourseNumber, user.GroupNumber)];
        foreach (var subject in group.Keys)
        {
            builder.Append(subject);
            builder.Append(": ");
            if (group.Timetable.ContainsKey(subject))
            {
                var timetable = group.Timetable[subject];
                var days = Join(", ", timetable.Select(ParseDayOfWeek));
                builder.Append(days);
            }
            else
            {
                builder.Append(ParseDayOfWeek(DayOfWeek.Sunday));
            }
            builder.AppendLine();
        }
        
        return new SendMessageRequest(id, builder.ToString());
    }
    
    private static string ParseDayOfWeek(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => "понедельник",
            DayOfWeek.Tuesday => "вторник",
            DayOfWeek.Wednesday => "среда",
            DayOfWeek.Thursday => "четверг",
            DayOfWeek.Friday => "пятница",
            DayOfWeek.Saturday => "суббота",
            _ => "воскресенье"
        };
    }
}