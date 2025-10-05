using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Validators;

namespace LabsQueueBot.DataAccess.Entities
{
    public class User
    {
        public required long Id { get; init; } = 0;
        public string Name { get; set; } = string.Empty; 
        public byte CourseNumber { get; set; } = 0;
        public byte GroupNumber { get; set; } = 0;
        public UserState State { get; set; } = UserState.None;
        public Role Role { get; set; } = Role.Default;
        public bool IsNotifyNeeded { get; set; } = true;
        public DateTime LastActivityAt { get; set; }
        public int? LastCallbackableMessageId { get; set; }
        public string? Username { get; set; }
        
        public User() {}

        public User(byte course, byte group, string name, long id) // TODO сделать что-то с конструктором имени пользователя из Ватаги
        {
            // StringBuilder builder = new StringBuilder();
            // if (course < 1 || course > 6)
            // {
            //     builder.AppendLine("Некорректный номер курса");
            // }
            //
            // if (group < 1 || group > 99)
            // {
            //     builder.AppendLine("Некорректный номер группы");
            // }
            //
            // if (name.Split(' ')[0].Trim().Length < 2)
            // {
            //     builder.AppendLine("Фамилия должна содержать как минимум две буквы");
            // }
            //
            // if (name.Split(' ')[1].Trim().Length < 2)
            // {
            //     builder.AppendLine("Имя должно содержать как минимум две буквы");
            // }
            //
            // if (name.Any(c => "0123456789~!@#$%^&*()_+{}:\"|?><`=[]\\;',./№".Contains(c)))
            // {
            //     builder.AppendLine("Имя и фамилия не должны содержать цифр и специальных символов");
            // }
            //
            // if (builder.Length != 0)
            // {
            //     throw new ArgumentException(builder.ToString());
            // }
            
            var validationResult = CourseGroupValidator.Validate(course, group);
            if (validationResult != null)
            {
                throw new ArgumentException(validationResult);
            }

            validationResult = UserInfoValidator.ValidateName(name);
            if (validationResult != null)
            {
                throw new ArgumentException(validationResult);
            }

            CourseNumber = course;
            GroupNumber = group;
            var first = name.IndexOf("👑");
            if (first != -1)
                name = name.Remove(first);
            first = name.IndexOf("Ватага");
            if (first != -1)
            {
                name = name[..first];
                name += "👑";
            }

            Name = name;
            Id = id;
        }
    }
}