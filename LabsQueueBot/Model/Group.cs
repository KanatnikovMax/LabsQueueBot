using System.Collections;

namespace LabsQueueBot
{
    /// <summary>
    /// Группа - хранилище очередей; <br/>
    /// представляет из себя словарь <br/>
    /// Название дисциплины : Очередь по дисциплине 
    /// </summary>
    public class Group : IEnumerable<KeyValuePair<string, Queue>>
    {
        /// <summary>
        /// Словарь <br/>
        /// Название дисциплины : Очередь по дисциплине 
        /// </summary>
        public Dictionary<string, Queue> _subjects;

        /// <summary>
        /// Номер курса
        /// </summary>
        public byte CourseNumber { get; set; }

        /// <summary>
        /// Номер группы
        /// </summary>
        public byte GroupNumber { get; set; }

        /// <summary>
        /// Количество студентов в группе
        /// </summary>
        public byte StudentsCount { get; private set; }

        /// <summary>
        /// Количество дисциплин в группе
        /// </summary>
        public int CountSubjects
        {
            get => _subjects.Count;
        }

        /// <summary>
        /// Конструктор класса Group; <br/>
        /// синхронизирует данные группы из БД
        /// </summary>
        /// <param name="course"> номер курса </param>
        /// <param name="number"> номер группы </param>
        public Group(byte course, byte number)
        {
            CourseNumber = course;
            GroupNumber = number;
            _subjects = new Dictionary<string, Queue>();
            using (var db = new QueueBotContext())
            {
                var collection = db.SubjectRepository
                    .Where(x => x.GroupNumber == number && x.CourseNumber == course)
                    .ToList();
                foreach (var subject in collection)
                {
                    _subjects.Add(subject.SubjectName, new Queue(course, number, subject.SubjectName, subject.Id));
                }
            }
        }

        /// <summary>
        /// Добавляет очередь по дисциплине; <br/>
        /// обновляет БД
        /// </summary>
        /// <param name="subject"> название дисциплины </param>
        public void AddSubject(string subject)
        {
            if (_subjects.ContainsKey(subject))
                throw new ArgumentException("Этот предмет уже есть в списке");
            if (CountSubjects == 20)
                throw new InvalidOperationException("Очередей в этой группе слишком много");
            int subjectId;
            using (var db = new QueueBotContext())
            {
                db.SubjectRepository.Add(
                    new Subject()
                    {
                        SubjectName = subject,
                        CourseNumber = CourseNumber,
                        GroupNumber = GroupNumber
                    });
                db.SaveChanges();
                subjectId = db.SubjectRepository
                    .FirstOrDefault(sb => sb.SubjectName == subject
                                          && sb.CourseNumber == CourseNumber
                                          && sb.GroupNumber == GroupNumber)
                    .Id;
            }

            _subjects.Add(subject, new Queue(subjectId));
        }

        /// <summary>
        /// Удаляет очередь по дисциплине; <br/>
        /// обновляет БД
        /// </summary>
        /// <param name="subject"> название дисциплины </param>
        /// <returns>
        /// true, если дисциплина была удалена; <br/>
        /// false, если дисциплина не была удалена
        /// </returns>
        public bool DeleteSubject(string subject)
        {
            if (_subjects.ContainsKey(subject))
            {
                using (var db = new QueueBotContext())
                {
                    var subj = db.SubjectRepository.FirstOrDefault(
                        x => x.SubjectName == subject
                             && x.GroupNumber == GroupNumber
                             && x.CourseNumber == CourseNumber);
                    if (subj is not null)
                    {
                        db.SubjectRepository.Remove(subj);
                        db.SaveChanges();
                    }
                }
                _subjects.Remove(subject);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Инкриминирует свойство StudentsCount, если оно меньше 50
        /// </summary>
        /// <returns>
        /// true, если StudentsCount было инкриминировано; <br/>
        /// false, если StudentsCount не было инкриминировано
        /// </returns>
        public bool AddStudent()
        {
            if (StudentsCount == 50)
                return false;
            StudentsCount++;
            return true;
        }

        /// <summary>
        /// Добавляет пользователя в очередь
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        /// <param name="subject"> название дисциплины </param>
        /// <returns>
        /// позицию пользователя в очереди; <br/>
        /// -1, если пользователь добавился в очередь (список ожидания распределения); <br/>
        /// -2, если пользователь находится в списке ожидания очереди
        /// </returns>
        public int AddStudent(long id, string subject)
        {
            int position = _subjects[subject].Position(id);
            if (position == -1)
                _subjects[subject].Add(id);
            return position;
        }

        /// <summary>
        /// Удаляет пользователя из очередей; <br/>
        /// очереди, ставшие пустыми, удаляются
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        public void RemoveStudent(long id)
        {
            foreach (var queue in _subjects)
            {
                queue.Value.Remove(id);
                if (queue.Value.Count == 0)
                    DeleteSubject(queue.Key);
            }

            StudentsCount--;
        }

        /// <summary>
        /// Удаляет пользователя из очереди; <br/>
        /// если очередь стала пустой - она удаляется
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        /// <param name="subject"> название дисциплины </param>
        /// <returns>
        /// true, если пользователь был удален; <br/>
        /// false, если пользователь не был удален
        /// </returns>
        public bool RemoveStudentFromQueue(long id, string subject)
        {
            var queue = _subjects[subject];
            if (queue.Remove(id))
            {
                if (queue.Count == 0)
                    DeleteSubject(subject);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Реализация Contains для дисциплин в группе
        /// </summary>
        /// <param name="key"> название дисциплины </param>
        public bool ContainsKey(string key) => _subjects.ContainsKey(key);

        /// <summary>
        /// Реализация GetEnumerator для класса Group
        /// </summary>
        public IEnumerator<KeyValuePair<string, Queue>> GetEnumerator() => _subjects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Реализация индексатора для класса Group
        /// </summary>
        /// <param name="key"> название дисциплины </param>
        public Queue this[string key] => _subjects[key];

        /// <summary>
        /// Вызывает Union для каждой очереди в группе
        /// </summary>
        public void Union()
        {
            foreach (var queue in _subjects.Values)
                queue.Union();
        }

        /// <summary>
        /// Реализация коллекции ключей группы
        /// </summary>
        public Dictionary<string, Queue>.KeyCollection Keys => _subjects.Keys;
    }
}