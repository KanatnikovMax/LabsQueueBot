namespace LabsQueueBot.Core.Enums;

/// <summary>
/// Перечисление прав, которые может иметь пользователь; <br/>
/// необходимо для работы комманд
/// </summary>
public enum Role
{
    Nobody,
    Default,
    Privileged,
    Admin
}