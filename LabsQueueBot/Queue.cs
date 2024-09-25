using System.Collections;
using System.Security.Cryptography;

namespace LabsQueueBot
{
    public class SerialNumber
    {
        public int Id { get; set; }
        public long UserIndex { get; set; }
    }

    public class Queue : IEnumerable<long>
    {
        public List<long> _queue = new(30);
        public List<long> _waiting = new(30);
        public Queue() { }
        public Queue(byte courseNumber, byte groupNumber, string subjectName)
        {
            CourseNumber = courseNumber;
            GroupNumber = groupNumber;
            SubjectName = subjectName;
        }

        public string SubjectName { get; set; }
        public byte CourseNumber { get; set; }
        public byte GroupNumber { get; set; }

        public int Count { get => _queue.Count + _waiting.Count; }

        /// <summary>
        /// ищет индекс студента в очереди
        /// </summary>
        /// <returns>
        /// положительный индекс, если нашёл; 
        /// -1, если не нашёл нигде; 
        /// -2, если находится в состоянии ожидания
        /// </returns>
        public int Position(long id)
        {
            var index = _queue.FindIndex(0, _queue.Count, val => val == id);
            return index >= 0 ? index 
                : (_waiting.FindIndex(0, _waiting.Count, val => val == id) >= 0 ? -2 : -1);
        }

        public void Add(User user) => _waiting.Add(user.Id);

        public void Clear()
        {
            _queue.Clear();
            _waiting.Clear();
        }

        public bool Remove(long id)
        {
            var index = Position(id);
            if (index >= 0)
            {
                _queue.RemoveAt(index);
                return true;
            }
            else
            {
                index = _waiting.FindIndex(0, _waiting.Count, val => val == id);
                if (index >= 0)
                {
                    _waiting.RemoveAt(index);
                    return true;
                }
            }
            return false;
        }

        public void Union()
        {
            while (_waiting.Count > 0)
            {
                int index = RandomNumberGenerator.GetInt32(0, _waiting.Count);
                _queue.Add(_waiting[index]);
                _waiting.RemoveAt(index);
            }
        }

        public IEnumerator<long> GetEnumerator() => _queue.GetEnumerator();      

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Skip(long id)
        {
            var index = Position(id);
            if (index == -1)
                throw new InvalidOperationException("Тебя тут нет, кого ты пропускаешь?");
            if (index == -2)
                throw new InvalidOperationException("Ты в списке ожидания, так чего не ждётся?");
            if (index == _queue.Count - 1)
                throw new InvalidOperationException("Ты уже итак в конце очереди, ожидай своего часа :)");
            (_queue[index], _queue[index + 1]) = (_queue[index + 1], _queue[index]);
        }
    }
}
