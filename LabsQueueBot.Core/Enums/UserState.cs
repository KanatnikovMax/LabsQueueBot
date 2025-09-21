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
    UnsetStudentData,
    Join,
    Quit,
    Skip,
    AddGroup,
    RefToAddSubject,
    AddSubject,
    Rename,
    ChooseGroup,
    ChangeData,
    Ban,
    Union,
    ShowTimetable,
    SetTimetable,
    SetTimetableDays,
    ShowWaiting
}