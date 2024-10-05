namespace LabsQueueBot
{
    /// <summary>
    /// Хранилище пользователей; <br/>
    /// представляет из себя словарь <br/>
    /// Id пользователя : Пользователь
    /// </summary>
    public static class Users
    {
        /// <summary>
        /// Словарь <br/> Id пользователя : Пользователь
        /// </summary>
        private static Dictionary<long, User> _users;

        /// <summary>
        /// Конструктор статического класса Users <br/>
        /// Синхронизирует данные пользователей из БД
        /// </summary>
        static Users()
        {
            _users = new Dictionary<long, User>();
            List<User> lst;
            using (var db = new QueueBotContext())
            {
                lst = db.UserRepository.ToList();
            }
            foreach (var user in lst)
            {
                _users[user.Id] = user;
                if (user.CourseNumber > 0 && user.GroupNumber > 0
                                          && !Groups.ContainsKey(new GroupKey(user.CourseNumber, user.GroupNumber)))
                {
                    Groups.Add(user.CourseNumber, user.GroupNumber);
                }
            }
        }

        /// <summary>
        /// Реализует Contains для пользователей в хранилище <br/>
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        public static bool Contains(long id) => _users.ContainsKey(id);

        /// <summary>
        /// Добавляет пользователя с id и именем
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        /// <param name="name"> имя пользователя </param>
        public static void Add(long id, string name)
        {
            User user;
            using (var db = new QueueBotContext())
            {
                user = new(name, id);
                db.UserRepository.Remove(new User(id));
                db.UserRepository.Add(user);
                db.SaveChanges();
            }
             
            _users.Remove(user.Id);           
            _users.Add(user.Id, user);
            
        }

        /// <summary>
        /// Обновляет данные пользователя в хранилище; <br/>
        /// обновляет БД
        /// </summary>
        /// <param name="user"> сущность-пользователь </param>
        public static void Add(User user)
        {
            var tmpUser = _users.Where(x => x.Key == user.Id);
            using (var db = new QueueBotContext())
            {
                if (tmpUser.Count() > 0)
                {
                    db.UserRepository.Remove(tmpUser.First().Value);
                }
                db.UserRepository.Add(user);
                db.SaveChanges();
            }

            if (tmpUser.Count() > 0)
            {
                _users.Remove(user.Id);
            }
            _users.Add(user.Id, user);
            
        }

        /// <summary>
        /// Удаляет пользователя из хранилища; <br/>
        /// обновляет БД
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        /// <returns>
        /// true, если пользователь был удален; <br/>
        /// false, если пользователь не был удален
        /// </returns>
        public static bool Remove(long id)
        {
            using (var db = new QueueBotContext())
            {
                db.UserRepository.Remove(_users[id]);
                db.SaveChanges();
            }

            return _users.Remove(id);
        }

        /// <summary>
        /// Реализует индексатор хранилища пользователей
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        public static User At(long id) => _users[id];

        /// <summary>
        /// Реализует коллекцию ключей хранилища
        /// </summary>
        public static Dictionary<long, User>.KeyCollection Keys => _users.Keys;
    }
}