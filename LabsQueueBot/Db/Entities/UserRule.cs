namespace LabsQueueBot.Db.Entities;

public class UserRule
{
    /// <summary>
    /// Перечисление прав, которые может иметь пользователь; <br/>
    /// необходимо для работы комманд
    /// </summary>
    public enum Rule
    {
        Use,
        SetTimetable,
        Random,
        Ban,
        Admin
    }
    
    /// <summary>
    /// Id пользователя
    /// </summary>
    public long Id { get; set; } = 0;
    
    /// <summary>
    /// Текущие права пользователя
    /// </summary>
    public Rule UsersRule { get; set; } = Rule.Use;
}