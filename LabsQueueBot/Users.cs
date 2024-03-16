using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LabsQueueBot
{
    internal static class Users
    {       
        // ID пользователя : Пользователь
        private static Dictionary<long, User> _users;
        static Users()
        {
            _users = new Dictionary<long, User>();
        }

        public static bool Contains(long id) => _users.ContainsKey(id);

        public static void Add(long id, string name) 
        {
            User user = new User(name, id);
            _users.Remove(user.ID);
            _users.Add(user.ID, user);
        }

        public static void Add(User user)
        {
            _users.Remove(user.ID);
            _users.Add(user.ID, user);

            //if (Groups.groups[new GroupKey(user.Course, user.Group)].StudentsCount > 99)
            //    return false;
            //TODO: перенести в StartApplier
            //Groups.groups[new GroupKey(user.Course, user.Group)].StudentsCount += 1;
            //return true;
        }

        public static bool Remove(long id)
        {
            //удаление  студента из группы
            if (_users.Remove(id))
            {
                //Groups.Remove(id);
                return true; 
            }
            return false;
        }
        public static User At(long id) => _users[id];
    }
}
