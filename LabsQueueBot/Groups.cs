using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabsQueueBot
{
    struct GroupKey : IComparable<GroupKey>
    {
        public byte Course { get; init; }
        public byte Number { get; init; }

        public GroupKey(byte course, byte number)
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

        public static bool operator>(GroupKey gk1, GroupKey gk2)
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

        public override int GetHashCode() => HashCode.Combine(Number,Course);
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

    internal static class Groups
    {
        internal static readonly Dictionary<GroupKey, Group> groups = new();

        private static int _groupsCount;
        public static int GroupsCount
        {
            get => groups.Count;
            private set => _groupsCount = value;
        }
        public static bool ContainsKey(GroupKey key) => groups.ContainsKey(key);

        //сделал удаление группы если удалили последнего ее студента
        public static bool Remove(long id)
        {
            GroupKey key = new GroupKey(Users.At(id).Course, Users.At(id).Group);
            if (!groups.ContainsKey(key))
                return false;
            groups[key].RemoveStudent(id);
            if (groups[key].StudentsCount == 0)
                groups.Remove(key);
            return true;
        }

        public static void Add(byte course, byte group) 
        {
            StringBuilder builder = new StringBuilder();
            if (course > 6 || course < 1)
                builder.AppendLine("Некорректный номер курса");
            if (group > 99 || group < 1)
                builder.AppendLine("Некорректный номер группы");
            if (builder.Length != 0)
                throw new ArgumentException(builder.ToString());

            GroupKey key = new GroupKey(course, group);
            if (groups.ContainsKey(key))
                throw new InvalidDataException("Такая группа уже существует");

            if (GroupsCount == 60)
                throw new InvalidOperationException("Много групп");
            GroupsCount++;
            groups.Add(key, new Group(course, group));
        }

        public static Group At(GroupKey key) => groups[key];

        public static string ShowQueue(long id, string subject)
        {
            GroupKey key = new GroupKey(Users.At(id).Course, Users.At(id).Group);
            if (!groups.ContainsKey(key))
                return $"Не существует {key.ToString()}";

            Group group = groups[key];
            if (!group.ContainsKey(subject))
                return "Эта очередь пуста";

            StringBuilder builder = new StringBuilder();
            int number = 1;

            foreach (var id in group[subject])
                builder.AppendLine($"{number++}. {Users.At(id).Name}");

            if (builder.Equals(""))
                builder.AppendLine("Эта очередь пуста");
            return builder.ToString();
        }

        public static string ShowSubjects(long id)
        {
            GroupKey key = new GroupKey(Users.At(id).Course, Users.At(id).Group);
            //if (!groups.ContainsKey(key))
            //    return "Вашей группы нет в списке\n/change_info чтобы изменить свои курс и группу";
            Group group = groups[key];
            StringBuilder builder = new StringBuilder();
            if (group.CountSubjects == 0)
                return "В вашей группе не открыто ни одной очереди\n"
                    + "/join чтобы создать очередь";
            builder.AppendLine("Очереди твоего курса и твои номера в них:");
            foreach (var queue in group)
            {
                //random_queue_update
                //исправил так, чтобы возвращало сразу место в очереди
                //ДОБАВИЛ
                var position = queue.Value.Position(id);
                //ВМЕСТО
                //var position = queue.Value.Position(id) + 1;

                //random_queue_update
                //ДОБАВИЛ
                //
                string text;
                if (position > 0)
                    text = position.ToString();
                else
                    if (position == 0)
                        text = "отсутствует";
                    else
                        text = "в ожидании";
                //ВМЕСТО
                //string text = position != 0 ? position.ToString() : "отсутствует";

                builder.AppendLine($"{queue.Key} -> {text}");
                //builder.AppendLine($"Предмет: {queue.Key}\nНомер в очереди: {text}");
            }
            return builder.ToString();
        }
        public static Dictionary<GroupKey, Group>.KeyCollection Keys => groups.Keys;
    }
}
