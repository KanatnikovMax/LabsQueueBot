namespace LabsQueueBot.Core.Enums;

/// <summary>
/// Перечисление состояний, в которых может находиться пользователь; <br/>
/// необходимо для работы контроллера
/// </summary>
public enum UserState
{
    None,
    ShowQueue,
    Unregistered,
    Register,
    Join,
    Quit,
    Skip,
    AddGroup,
    AddSubject,
    Rename,
    ChooseGroup,
    Ban,
    Union,
    ShowTimetable,
    SetTimetable,
    SetTimetableDays
}