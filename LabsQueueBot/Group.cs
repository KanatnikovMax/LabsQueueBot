using System.Collections;

namespace LabsQueueBot
{
    internal class Group : IEnumerable<KeyValuePair<string, Queue>>
    {
        // название дисциплины : очередь по дисциплине
        public Dictionary<string, Queue> _subjects;

        private byte _course;
        public byte CourseNumber
        {
            get => _course;
            set
            {
                _course = value;
            }
        }
        public byte StudentsCount { get; private set; }
        public int CountSubjects { get => _subjects.Count; }
        public byte GroupNumber { get; set; }
        public void AddSubject(string subject)
        {
            if (_subjects.ContainsKey(subject))
                throw new ArgumentException("Этот предмет уже есть в списке");
            if (CountSubjects == 20)
                throw new InvalidOperationException("Очередей в этой группе слишком много");
            _subjects.Add(subject, new Queue());
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
            }
        }
        public bool DeleteSubject(string subject)
        {
            if (_subjects.Remove(subject))
            {
                using (var db = new QueueBotContext())
                {
                    var subj = db.SubjectRepository.FirstOrDefault(
                        x => x.SubjectName == subject
                        && x.GroupNumber == GroupNumber
                        && x.CourseNumber == CourseNumber);
                    if (subj != null)
                    {
                        db.SubjectRepository.Remove(subj);
                        db.SaveChanges();
                    }
                }
                return true;
            }
            return false;
        }
        public Group(byte course, byte number)
        {
            _course = course;
            GroupNumber = number;
            _subjects = new Dictionary<string, Queue>();
            using (var db = new QueueBotContext())
            {
                var collection = db.SubjectRepository
                    .Where(x => x.GroupNumber == number && x.CourseNumber == course)
                    .ToList();
                foreach (var subject in collection)
                {
                    _subjects.Add(subject.SubjectName, new Queue(course, number, subject.SubjectName));
                }
            }

        }

        public bool AddStudent()
        {
            if (StudentsCount == 50)
                return false;
            StudentsCount++;
            return true;
        }
        // -1, если не нашли студента, -2, если нашли в списке ожидающих, >0, если уже в очереди
        public int AddStudent(long id, string subject)
        {
            int position = _subjects[subject].Position(id);
            if (position == -1)
                _subjects[subject].Add(Users.At(id));
            return position;
        }

        public void RemoveStudent(long id)
        {
            foreach (var queue in _subjects)
            {
                queue.Value.Remove(id);
                if (queue.Value.Count == 0)
                    DeleteSubject(queue.Key);
            }
            StudentsCount -= 1;
        }
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
        public bool ContainsKey(string key) => _subjects.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, Queue>> GetEnumerator()
        {
            return _subjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Queue this[string key]
        {
            get => _subjects[key];
        }

        public void Union()
        {
            foreach(var queue in _subjects.Values)
                queue.Union();
        }
        public Dictionary<string, Queue>.KeyCollection Keys { get => _subjects.Keys; }
    }
}
