namespace LabsQueueBot.Core.Settings;

public class CommandsSettings
{
    public (string Type, string Name, string Definition) HelpCommand { get; set; }
    public (string Type, string Name, string Definition) SwitchNotificationCommand { get; set; }
    public (string Type, string Name, string Definition) SetTimetableCommand { get; set; }
    public (string Type, string Name, string Definition) ShowTimetableCommand { get; set; }
    public (string Type, string Name, string Definition) ShowSubjectsCommand { get; set; }
    public (string Type, string Name, string Definition) ShowQueueCommand { get; set; }
    public (string Type, string Name, string Definition) ShowWaitingCommand { get; set; }
    public (string Type, string Name, string Definition) AddSubjectCommand { get; set; }
    public (string Type, string Name, string Definition) JoinCommand { get; set; }
    public (string Type, string Name, string Definition) QuitCommand { get; set; }
    public (string Type, string Name, string Definition) SkipCommand { get; set; }
    public (string Type, string Name, string Definition) UnionQueueCommand { get; set; }
    public (string Type, string Name, string Definition) SetGroupCommand { get; set; }
    public (string Type, string Name, string Definition) RenameCommand { get; set; }
    public (string Type, string Name, string Definition) StartCommand { get; set; }
    public (string Type, string Name, string Definition) StopCommand { get; set; }
}