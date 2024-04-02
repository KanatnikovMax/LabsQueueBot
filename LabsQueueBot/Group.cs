using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabsQueueBot
{
    internal class Group : IEnumerable<KeyValuePair<string, Queue>>, ICollection<KeyValuePair<string, Queue>>
    {
        

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
        public byte Number { get; set; }
        [JsonIgnore]
        public int CountSubjects { get => _subjects.Count; }
        
        // название дисциплины : очередь по дисциплине
        [JsonProperty]
        private Dictionary<string, Queue> _subjects;
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
        public Group() { }
        
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

        public void Union()
        {
            foreach(var queue in _subjects.Values)
                queue.Union();
        }

        public void Add(KeyValuePair<string, Queue> item)
        {
            _subjects.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _subjects.Clear();
        }

        public bool Contains(KeyValuePair<string, Queue> item)
        {
            return _subjects.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, Queue>[] array, int arrayIndex)
        {
            Array.Copy(_subjects.ToArray(), array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, Queue> item)
        {
            return _subjects.Remove(item.Key);
        }

        [JsonIgnore]
        public Dictionary<string, Queue>.KeyCollection Keys { get => _subjects.Keys; }

        [JsonIgnore]
        public int Count => _subjects.Count;
        [JsonIgnore]
        public bool IsReadOnly => true;
    }
}
