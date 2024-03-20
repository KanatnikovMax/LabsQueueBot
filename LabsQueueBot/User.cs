using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabsQueueBot
{ 
    internal class User
    {
        public enum UserState
        {
            //
            None, ShowQueue,
            //
            Unregistred, UnsetStudentData,
            //
            Join, Quit, Skip, AddGroup, AddSubject, Rename, ChangeData
        }
        //TODO: сделать состояния для очереди (ожидает/уже в очереди типа того)
        public byte Group { get; set; }
        public byte Course { get; set; }
        public string Name { get; set; }
        public readonly long ID;
        public UserState State { get; set; } = UserState.None;
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
            Course = course;
            Group = group;
            int first = name.IndexOf("👑");
            if (first != -1)
                name = name.Remove(first);
            first = name.IndexOf("Ватага");
            if (first != -1)
            {
                name = name[..first];
                name += "👑";
            }
            //"🌈";
            Name = name;
            ID = id;
        }      
        public User(long id) => ID = id;
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
            Course = 0;
            Group = 0;
            Name = name;
            ID = id;
        }
    }
}
