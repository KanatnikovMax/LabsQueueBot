using System.Text;

namespace LabsQueueBot
{
    /// <summary>
    /// Пара курс-группа для однозначной идентификации группы пользователей;
    /// для структуры переопределены операторы сравнения
    /// </summary>
    public struct GroupKey : IComparable<GroupKey>
    {
        public byte? Course { get; init; }
        public byte? Number { get; init; }

        public GroupKey(byte? course, byte? number)
        {
            Course = course;
            Number = number;
        }

        public static bool operator ==(GroupKey gk1, GroupKey gk2)
        {
            return gk1.Course == gk2.Course && gk1.Number == gk2.Number;
        }

        public static bool operator !=(GroupKey gk1, GroupKey gk2)
        {
            return gk1.Course != gk2.Course || gk1.Number != gk2.Number;
        }

        public static bool operator >(GroupKey gk1, GroupKey gk2)
        {
            if (gk1.Course > gk2.Course)
                return true;
            if (gk1.Course < gk2.Course)
                return false;
            return gk1.Number > gk2.Number;
        }

        public static bool operator >=(GroupKey gk1, GroupKey gk2)
        {
            return gk1 > gk2 || gk1 == gk2;
        }

        public static bool operator <(GroupKey gk1, GroupKey gk2)
        {
            if (gk1.Course < gk2.Course)
                return true;
            if (gk1.Course > gk2.Course)
                return false;
            return gk1.Number < gk2.Number;
        }

        public static bool operator <=(GroupKey gk1, GroupKey gk2)
        {
            return gk1 < gk2 || gk1 == gk2;
        }

        public bool Equals(GroupKey other) => other == this;

        public override int GetHashCode() => HashCode.Combine(Number, Course);
        public override string ToString() => $"{Course} курс {Number} группа";

        public int CompareTo(GroupKey other)
        {
            if (this > other)
                return 1;
            if (this < other)
                return -1;
            return 0;
        }
    }

    /// <summary>
    /// Статическое хранилище групп пользователей; <br/>
    /// содержит в себе все существующие группы
    /// </summary>
    public static class Groups
    {
        /// <summary>
        /// Словарь <br/> Номер курса-группы : группа
        /// </summary>
        public static readonly Dictionary<GroupKey, Group> groups = new();

        private static int _groupsCount;

        /// <summary>
        /// Общее количество групп
        /// </summary>
        public static int GroupsCount
        {
            get => groups.Count;
            private set => _groupsCount = value;
        }

        /// <summary>
        /// Реализует Contains для групп в хранилище <br/>
        /// </summary>
        /// <param name="key"> номер курса-группы </param>
        public static bool ContainsKey(GroupKey key) => groups.ContainsKey(key);

        /// <summary>
        /// Удаляет пользователя из его группы; <br/>
        /// если группа стала пустой - она удаляется
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        /// <returns>
        /// true, если пользователь был удален; <br/>
        /// false, если пользователь не был удален
        /// </returns>
        public static bool Remove(long id)
        {
            GroupKey key = new GroupKey(Users.At(id).CourseNumber, Users.At(id).GroupNumber);
            if (!groups.ContainsKey(key))
                return false;
            groups[key].RemoveStudent(id);
            if (groups[key].StudentsCount == 0)
                groups.Remove(key);
            return true;
        }

        /// <summary>
        /// Добавляет новую группу
        /// </summary>
        /// <param name="course"> номер курса </param>
        /// <param name="group"> номер группы </param>
        /// <exception cref="ArgumentException">
        /// если некорректен номер курса или группы
        /// </exception>
        /// /// <exception cref="InvalidDataException">
        /// если группа с такими параметрами уже существует
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// если количество групп превышает максимальное значение (60)
        /// </exception>
        public static void Add(byte course, byte group)
        {
            var builder = new StringBuilder();
            if (course > 6 || course < 1)
                builder.AppendLine("Некорректный номер курса");
            if (group > 99 || group < 1)
                builder.AppendLine("Некорректный номер группы");
            if (builder.Length != 0)
                throw new ArgumentException(builder.ToString());

            var key = new GroupKey(course, group);
            if (groups.ContainsKey(key))
                throw new InvalidDataException("Такая группа уже существует");

            if (GroupsCount == 60)
                throw new InvalidOperationException("Много групп");
            GroupsCount++;
            groups.Add(key, new Group(course, group));
        }

        /// <summary>
        /// Реализует индексатор хранилища групп
        /// </summary>
        /// <param name="key"> номер курса-группы </param>
        public static Group At(GroupKey key) => groups[key];

        /// <summary>
        /// Возвращает строку с очередью по дисциплине в группе пользователя
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        /// <param name="subject"> название дисциплины </param>
        /// <returns>
        /// строку, содержащую очередь по выбранной дисциплине группы, в которой находится выбранный пользователь
        /// </returns>
        public static string ShowQueue(long id, string subject)
        {
            var key = new GroupKey(Users.At(id).CourseNumber, Users.At(id).GroupNumber);
            if (!groups.ContainsKey(key))
                return $"Не существует {key.ToString()}";

            var group = groups[key];
            if (!group.ContainsKey(subject))
                return "Эта очередь пуста";

            var builder = new StringBuilder();
            int number = 1;

            foreach (var userId in group[subject])
                builder.AppendLine($"{number++}. {Users.At(userId).Name}");

            if (builder.Equals(""))
                builder.AppendLine("Эта очередь пуста");
            return builder.ToString();
        }

        /// <summary>
        /// Возвращает строку с дисциплинами в группе пользователя
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        /// <returns>
        /// строку, содержащую все дисциплины группы, в которой находится выбранный пользователь
        /// </returns>
        public static string ShowSubjects(long id)
        {
            var key = new GroupKey(Users.At(id).CourseNumber, Users.At(id).GroupNumber);
            var group = groups[key];
            var builder = new StringBuilder();
            if (group.CountSubjects == 0)
                return "В вашей группе не открыто ни одной очереди\n"
                       + "/join чтобы создать очередь";
            builder.AppendLine("Очереди твоего курса и твои номера в них:");
            foreach (var queue in group)
            {
                var position = queue.Value.Position(id) + 1;
                string text = position > 0 ? position.ToString() : (position == 0 ? "отсутствует" : "в ожидании");
                builder.AppendLine($"{queue.Key} -> {text}");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Вызывает Union для каждой группы
        /// </summary>
        public static void Union()
        {
            foreach (var group in groups.Values)
                group.Union();
        }

        /// <summary>
        /// Реализует коллекцию ключей хранилища
        /// </summary>
        public static Dictionary<GroupKey, Group>.KeyCollection Keys => groups.Keys;
    }
}