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
    
    public static string ToString(WeekDays weekDay)
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

    public static string ToString(IEnumerable<WeekDays> weekDays)
        => weekDays.Select(ToString).Aggregate((left, right) => $"{left} {right}");

    public static IEnumerable<WeekDays> ParseWeekDays(WeekDays weekDays)
    {
        var result = new List<WeekDays>();
        
        if (weekDays.HasFlag(WeekDays.Monday)) result.Add(WeekDays.Monday);
        if (weekDays.HasFlag(WeekDays.Tuesday)) result.Add(WeekDays.Tuesday);
        if (weekDays.HasFlag(WeekDays.Wednesday)) result.Add(WeekDays.Wednesday);
        if (weekDays.HasFlag(WeekDays.Thursday)) result.Add(WeekDays.Thursday);
        if (weekDays.HasFlag(WeekDays.Friday)) result.Add(WeekDays.Friday);
        if (weekDays.HasFlag(WeekDays.Saturday)) result.Add(WeekDays.Saturday);
        if (weekDays.HasFlag(WeekDays.Sunday)) result.Add(WeekDays.Sunday);

        return result;
    }
}