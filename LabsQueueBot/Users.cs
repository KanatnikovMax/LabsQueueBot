﻿namespace LabsQueueBot
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
            User user = new (name, id);
            _users.Remove(user.Id);
            _users.Add(user.Id, user);
        }

        public static void Add(User user)
        {
            _users.Remove(user.Id);
            _users.Add(user.Id, user);
        }

        public static bool Remove(long id)
        {
            //удаление  студента из группы
            return _users.Remove(id);
        }
        public static User At(long id) => _users[id];
        public static Dictionary<long, User>.KeyCollection Keys => _users.Keys;
    }
}
