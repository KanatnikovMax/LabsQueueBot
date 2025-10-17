namespace LabsQueueBot.Web.Providers;

public interface IWeekProvider
{
    bool IsNumWeek { get; }
    void SwitchWeekNumerate();
    string GetCurrentWeekName();
}