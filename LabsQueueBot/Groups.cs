using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabsQueueBot
{
    struct GroupKey
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
            return gk1.Course == gk2.Course && gk1.Number == gk1.Number;
        }

        public static bool operator !=(GroupKey gk1, GroupKey gk2)
        {
            return gk1.Course != gk2.Course || gk1.Number != gk2.Number;
        }

        public bool Equals(GroupKey other) => other == this;

        public override int GetHashCode() => HashCode.Combine(Number,Course);
    }

    internal static class Groups
    {
        //TODO: здесь должны храниться Group
        internal static readonly Dictionary<GroupKey, Group> groups = new()
        {
            {new GroupKey() { Course = 2, Number = 9  }, new Group(2, 9) },
            {new GroupKey() { Course = 2, Number = 91 }, new Group(2, 91) }
        };

        //сделал удаление группы если удалили последнего ее студента
        public static bool Remove(long id)
        {
            GroupKey key = new GroupKey(Users.At(id).Course, Users.At(id).Group);
            if (!groups.ContainsKey(key))
                return false;
            groups[key].RemoveUser(id);
            groups[key].StudentsCount -= 1;
            if (groups[key].StudentsCount == 0)
                groups.Remove(key);
            return true;
        }


        public static Group At(GroupKey key) => groups[key];

        public static string ShowQueue(long id, string subject)
        {
            
            GroupKey key = new GroupKey(Users.At(id).Course, Users.At(id).Group);
            if (!groups.ContainsKey(key))
                return "";
            Group group = groups[key];
            if (group.ContainsKey(subject))
                return "";
            StringBuilder builder = new StringBuilder();
            //TODO: выбрать один из циклов и определить возвращаемые значения
            //for (int number = 1; number < group[subject].Count; ++number)
            //{
            //    builder.AppendLine($"{number}. {group[subject][number].Name}");
            //}
            int number = 1;
            foreach (var user in group[subject])
            {
                builder.AppendLine($"{number++}. {user.Name}");
                //++number;
            }
            if (builder.Equals(""))
            {
                builder.AppendLine("Очередь пуста!\nВоин, запишись первым");
            }
            return builder.ToString();
        }

        public static string ShowSubjects(long id)
        {
            GroupKey key = new GroupKey(Users.At(id).Course, Users.At(id).Group);
            if (!groups.ContainsKey(key))
                return "Вашей группы нет в списке\n/add_group чтобы добавить группу";
            Group group = groups[key];
            StringBuilder builder = new StringBuilder();
            // TODO: сделать проверку, что для группы не зарегистрировано ни одного предмета
            if (group.CountSubjects == 0)
                return "В вашей группе не открыто ни одной очереди\n" +
                    "/add_queue чтобы создать очередь";
            foreach (var queue in group)
            {
                var position = queue.Value.Position(id) + 1;
                string text = position != 0 ? position.ToString() : "отсутствует";
                builder.AppendLine($"{queue.Key} -> {text}");
                builder.AppendLine($"Предмет: {queue.Key}\nНомер в очереди: {text}");
            }
            return builder.ToString();
        }
    }
}
