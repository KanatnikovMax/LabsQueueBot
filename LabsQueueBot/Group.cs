using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabsQueueBot
{
    internal class Group : IEnumerable<KeyValuePair<string, Queue>>
    {
        // название дисциплины : очередь по дисциплине
        private readonly Dictionary<string, Queue> _subjects;

        private byte _course;
        public byte Course
        {
            get => _course;
            set
            {
                _course = value;
                ClearSubjects();
            }
        }
        public byte StudentsCount { get; private set; }
        public int CountSubjects { get => _subjects.Count; }
        public byte Number { get; set; }
        public void AddSubject(string subject)
        {
            if (_subjects.ContainsKey(subject))
                throw new ArgumentException("Этот предмет уже есть в списке");
            if (CountSubjects == 20)
                throw new InvalidOperationException("Очередей в этой группе слишком много");
            _subjects.Add(subject, new Queue());
        }
        public bool DeleteSubject(string subject)
        {
            return _subjects.Remove(subject);
        }
        public Group(byte course, byte number)
        {
            _course = course;
            Number = number;
            _subjects = new Dictionary<string, Queue>();
        }

        public bool AddStudent()
        {
            if (StudentsCount == 50)
                return false;
            StudentsCount++;
            return true;
        }

        public bool AddStudent(long id, string subject)
        {
            if (_subjects[subject].Position(id) != -1)
                return false;
            _subjects[subject].Add(Users.At(id));
            return true;
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

        public void ClearSubjects()
        {
            _subjects.Clear();
            StudentsCount = 0;
        }

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

        public Dictionary<string, Queue>.KeyCollection Keys { get => _subjects.Keys; }
    }
}
