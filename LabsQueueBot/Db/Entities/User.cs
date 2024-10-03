using System.Text;

namespace LabsQueueBot
{
    /// <summary>
    /// Сущность пользователя для хранения в БД; <br/>
    /// хранит в себе данные о пользователе: Id, имя и фамилию, номер курса и группы, состояние
    /// </summary>
    public class User
    {
        /// <summary>
        /// Перечисление состояний, в которых может находиться пользователь; <br/>
        /// необходимо для работы контроллера
        /// </summary>
        public enum UserState
        {
            None,
            ShowQueue,
            Unregistred,
            UnsetStudentData,
            Join,
            Quit,
            Skip,
            AddGroup,
            AddSubject,
            Rename,
            ChangeData
        }

        /// <summary>
        /// Id пользователя
        /// </summary>
        public long Id { get; set; } = 0;

        /// <summary>
        /// Имя-фамилия пользователя
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Номер курса пользователя
        /// </summary>
        public byte CourseNumber { get; set; } = 0;

        /// <summary>
        /// Номер группы пользователя
        /// </summary>
        public byte GroupNumber { get; set; } = 0;

        /// <summary>
        /// Текущее состояние пользователя
        /// </summary>
        public UserState State { get; set; } = UserState.None;

        /// <summary>
        /// Флаг необходимости в уведомлениях
        /// </summary>
        public bool IsNotifyNeeded { get; set; } = true;

        /// <summary>
        /// Конструктор класса User; <br/>
        /// полностью инициализирует сущность пользователя
        /// </summary>
        /// <param name="course"> номер курса пользователя </param>
        /// <param name="group"> номер группы пользователя </param>
        /// <param name="name"> имя-фамилия пользователя </param>
        /// <param name="id"> Id пользователя </param>
        /// <exception cref="ArgumentException">
        /// в случае, если: <br/>
        /// некорректен номер курса; <br/>
        /// некорректен номер группы; <br/>
        /// имя или фамилия состоит менее, чем из 2 символов; <br/>
        /// имя или фамилия содержат цифры или специальные символы
        /// </exception>
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

        /// <summary>
        /// Конструктор класса User; <br/>
        /// инициализирует Id пользователя
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        public User(long id) => Id = id;

        /// <summary>
        /// Конструктор класса User; <br/>
        /// инициализирует Id, имя и фамилию пользователя
        /// </summary>
        /// <param name="name"> имя и фамилия пользователя </param>
        /// <param name="id"> Id пользователя </param>
        /// <exception cref="ArgumentException">
        /// в случае, если: <br/>
        /// имя или фамилия состоит менее, чем из 2 символов; <br/>
        /// имя или фамилия содержат цифры или специальные символы
        /// </exception>
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