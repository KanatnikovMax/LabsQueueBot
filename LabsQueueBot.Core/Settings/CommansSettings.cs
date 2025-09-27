namespace LabsQueueBot.Core.Settings;

public class CommansSettings
{
    public required CommandSettings Help { get; init; }
    public required CommandSettings SwitchNotification { get; init; }
    public required CommandSettings SetTimetable { get; init; }
    public required CommandSettings ShowTimetable{ get; init; }
    public required CommandSettings ShowSubjects { get; init; }
    public required CommandSettings ShowQueue { get; init; }
    public required CommandSettings ShowWaiting { get; init; }
    public required CommandSettings AddSubject { get; init; }
    public required CommandSettings Join { get; init; }
    public required CommandSettings Quit { get; init; }
    public required CommandSettings Skip { get; init; }
    public required CommandSettings Union { get; init; }
    public required CommandSettings SetGroup { get; init; }
    public required CommandSettings Rename { get; init; }
    public required CommandSettings Start { get; init; }
    public required CommandSettings Stop { get; init; }
}