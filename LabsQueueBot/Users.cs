namespace LabsQueueBot
{
    public static class Users
    {       
        // ID пользователя : Пользователь
        private static Dictionary<long, User> _users;
        static Users()
        {
            _users = new Dictionary<long, User>();
            using QueueBotContext db = new();
            foreach (var user in db.UserRepository.ToList())
            {
                _users[user.Id] = user;
                if (user.CourseNumber > 0 && user.GroupNumber > 0
                    && !Groups.ContainsKey(new GroupKey(user.CourseNumber, user.GroupNumber)))
                {
                    Groups.Add(user.CourseNumber, user.GroupNumber);
                }
            }
        }

        public static bool Contains(long id) => _users.ContainsKey(id);

        public static void Add(long id, string name) 
        {
            using QueueBotContext db = new();
            User user = new (name, id);
            _users.Remove(user.Id);
            db.UserRepository.Remove(new User(id));
            _users.Add(user.Id, user);
            db.UserRepository.Add(user);
            db.SaveChanges();
        }

        public static void Add(User user)
        {
            using QueueBotContext db = new();
            var tmpUser = _users.Where(x => x.Key == user.Id);
            if (tmpUser.Count() > 0)
            {
                db.UserRepository.Remove(tmpUser.First().Value);
                _users.Remove(user.Id);
            }
            _users.Add(user.Id, user);
            db.UserRepository.Add(user);
            db.SaveChanges();
        }

        public static bool Remove(long id)
        {           
            using (QueueBotContext db = new())
            {
                db.UserRepository.Remove(_users[id]);
                db.SaveChanges();
            }
            return _users.Remove(id);
        }
        public static User At(long id) => _users[id];
        public static Dictionary<long, User>.KeyCollection Keys => _users.Keys;
    }
}
