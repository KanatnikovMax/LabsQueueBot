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
        private byte _course;
        public byte Course 
        {
            get => _course;
            set
            {
                _course = value;
                _subjects?.Clear();
            }
        }
        public byte StudentsCount { get; set; }
        public int CountSubjects { get => _subjects.Count; }
        public byte Number { get; set; }    
        // название дисциплины : очередь по дисциплине
        private readonly Dictionary<string, Queue> _subjects;
        public bool AddSubject(string subject)
        {
            if (_subjects.ContainsKey(subject))
                return false;
            _subjects.Add(subject, new Queue());
            return true;
        }
        public bool DeleteSubject(string subject)
        {
            return _subjects.Remove(subject);
        }
        public Group(byte course, byte number)
        {
            Course = course;
            Number = number;
            _subjects = new Dictionary<string, Queue>();
        }
        
        public void RemoveUser(long id)
        {
            foreach(var queue in _subjects.Values)
            {
                queue.Remove(id);
            }
        }
        public bool ContainsKey(string key) => _subjects.ContainsKey(key);

        public void ClearSubjects()
        {
            _subjects.Clear();
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

        public Dictionary<string, Queue>.ValueCollection Values => _subjects.Values;
        public Dictionary<string, Queue>.KeyCollection Keys => _subjects.Keys;
    }
}
