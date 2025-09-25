using LabsQueueBot.Core.Settings;

namespace LabsQueueBot.Web.SettingsReader;

public static class CommandsSettingsReader
{
    public static CommandsSettings Read(IConfiguration configuration)
    {
        var commandsSettings = new CommandsSettings();

        List<string> stringValues = [];
        
        configuration.GetSection("LabsQueueBot").GetSection("InlineKeyboardCreator").Bind(stringValues);
        commandsSettings.InlineKeyboardCreator = (stringValues[0], stringValues[1]);
        
        var section = configuration.GetSection("LabsQueueBot").GetSection("Commands");
        
        stringValues = [];
        section.GetSection("HelpCommand").Bind(stringValues);
        commandsSettings.HelpCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("SwitchNotificationCommand").Bind(stringValues);
        commandsSettings.SwitchNotificationCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("SetTimetableCommand").Bind(stringValues);
        commandsSettings.SetTimetableCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("ShowTimetableCommand").Bind(stringValues);
        commandsSettings.ShowTimetableCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("ShowSubjectsCommand").Bind(stringValues);
        commandsSettings.ShowSubjectsCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("ShowQueueCommand").Bind(stringValues);
        commandsSettings.ShowQueueCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("ShowWaitingCommand").Bind(stringValues);
        commandsSettings.ShowWaitingCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("AddSubjectCommand").Bind(stringValues);
        commandsSettings.AddSubjectCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("JoinCommand").Bind(stringValues);
        commandsSettings.JoinCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("QuitCommand").Bind(stringValues);
        commandsSettings.QuitCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("SkipCommand").Bind(stringValues);
        commandsSettings.SkipCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("UnionQueueCommand").Bind(stringValues);
        commandsSettings.UnionQueueCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("RenameCommand").Bind(stringValues);
        commandsSettings.RenameCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("SetGroupCommand").Bind(stringValues);
        commandsSettings.SetGroupCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("StartCommand").Bind(stringValues);
        commandsSettings.StartCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        stringValues = [];
        section.GetSection("StopCommand").Bind(stringValues);
        commandsSettings.StopCommand = (stringValues[0], stringValues[1], stringValues[2]);
        
        return commandsSettings;
    }
}