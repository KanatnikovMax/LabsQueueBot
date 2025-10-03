using LabsQueueBot.Core.Enums;

namespace LabsQueueBot.Core.Helpers;

public static class WeekDaysHelper
{
    public static WeekDays Parse(string weekDay)
    {
        return weekDay.ToLower() switch
        {
            "1" or "пн" or "понедельник" => WeekDays.Monday,
            "2" or "вт" or "вторник" => WeekDays.Tuesday,
            "3" or "ср" or "среда" => WeekDays.Wednesday,
            "4" or "чт" or "четверг" => WeekDays.Thursday,
            "5" or "пт" or "пятница" => WeekDays.Friday,
            "6" or "сб" or "суббота" => WeekDays.Saturday,
            "7" or "вс" or "воскресенье" => WeekDays.Sunday,
            _ => WeekDays.None
        };
    }

    public static string ToString(this WeekDays weekDay)
    {
        return weekDay switch
        {
            WeekDays.Monday => "пн",
            WeekDays.Tuesday => "вт",
            WeekDays.Wednesday => "ср",
            WeekDays.Thursday => "чт",
            WeekDays.Friday => "пт",
            WeekDays.Saturday => "сб",
            WeekDays.Sunday => "вс",
            _ => ""
        };
    }
}