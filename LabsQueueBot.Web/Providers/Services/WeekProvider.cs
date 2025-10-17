using LabsQueueBot.Core.Settings;
using Microsoft.Extensions.Options;

namespace LabsQueueBot.Web.Providers.Services;

public class WeekProvider(IOptions<WeekSettings> options) : IWeekProvider
{
    public bool IsNumWeek { get; private set; } = options.Value.IsNumWeek;

    public void SwitchWeekNumerate()
    {
        IsNumWeek = !IsNumWeek;
    }

    public string GetCurrentWeekName()
    {
        return IsNumWeek ? "числитель" : "знаменатель";
    }
}