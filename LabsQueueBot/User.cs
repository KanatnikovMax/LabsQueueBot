using System.Text;

namespace LabsQueueBot
{ 
    public class User
    {
        public enum UserState
        {
            
            None, ShowQueue,           
            Unregistred, UnsetStudentData,            
            Join, Quit, Skip, AddGroup, AddSubject, Rename, ChangeData
        }
        public long Id { get; set; } = 0;
        public string Name { get; set; } = String.Empty;
        public byte CourseNumber { get; set; } = 0;
        public byte GroupNumber { get; set; } = 0;     
        public UserState State { get; set; } = UserState.None;
        public bool IsNotifyNeeded { get; set; } = true;
        public User(byte course, byte group, string name, long id)
        {
            StringBuilder builder = new StringBuilder();
            if (course < 1 || course > 6)
            {
                builder.AppendLine("Некорректный номер курса");
            }
            if (group < 1 || group > 99)
            {
                builder.AppendLine("Некорректный номер группы");
            }
            if (name.Split(' ')[0].Trim().Length < 2)
            {
                builder.AppendLine("Фамилия должна содержать как минимум две буквы");
            }
            if (name.Split(' ')[1].Trim().Length < 2)
            {
                builder.AppendLine("Имя должно содержать как минимум две буквы");
            }
            if (name.Any(c => "0123456789~!@#$%^&*()_+{}:\"|?><`=[]\\;',./№".Contains(c)))
            {
                builder.AppendLine("Имя и фамилия не должны содержать цифр и специальных символов");
            }
            if (builder.Length != 0)
            {
                throw new ArgumentException(builder.ToString());
            }
            CourseNumber = course;
            GroupNumber = group;
            int first = name.IndexOf("👑");
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
        public User(long id) => Id = id;
        public User(string name, long id)
        {
            StringBuilder builder = new StringBuilder();
            if (name.Split(' ')[0].Length < 2)
            {
                builder.AppendLine("Фамилия должна содержать как минимум две буквы");
            }
            if (name.Split(' ')[1].Length < 2)
            {
                builder.Append("Имя должно содержать как минимум две буквы");
            }
            if (name.Any(c => "0123456789~!@#$%^&*()_+{}:\"|?><`=[]\\;',./№".Contains(c)))
            {
                builder.AppendLine("Имя и фамилия не должны содержать цифр и специальных символов");
            }
            if (builder.Length != 0)
            {
                throw new ArgumentException(builder.ToString());
            }
            CourseNumber = 0;
            GroupNumber = 0;
            Name = name;
            Id = id;
        }
    }
}
