using System.Collections;
using System.Security.Cryptography;

namespace LabsQueueBot
{
    internal class Queue : IEnumerable<User>
    {
        private readonly List<User> _data = new(30);
        private readonly List<User> _waiting = new(30);

        public int Count { get => _data.Count + _waiting.Count; }
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
            var index = _data.FindIndex(0, _data.Count, val => val.Id == id);
            return index >= 0 ? index : (_waiting.FindIndex(0, _waiting.Count, val => val.Id == id) >= 0 ? -2 : -1);
        }
        public void Add(User user) => _waiting.Add(user);
        public void Clear()
        {
            _data.Clear();
            _waiting.Clear();
        }
        public bool Remove(long id)
        {
            var index = Position(id);
            if (index >= 0)
            {
                _data.RemoveAt(index);
                return true;
            }
            else
            {
                index = _waiting.FindIndex(0, _waiting.Count, val => val.Id == id);
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
                _data.Add(_waiting[index]);
                _waiting.RemoveAt(index);
            }
        }
        public IEnumerator<User> GetEnumerator() => _data.GetEnumerator();      

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Skip(long id)
        {
            var index = Position(id);
            if (index == -1)
                throw new InvalidOperationException("Тебя тут нет, кого ты пропускаешь?");
            if (index == -2)
                throw new InvalidOperationException("Ты в списке ожидания, так чего не ждётся?");
            if (index == _data.Count - 1)
                throw new InvalidOperationException("Ты уже итак в конце очереди, ожидай своего часа :)");
            (_data[index], _data[index + 1]) = (_data[index + 1], _data[index]);
        }
    }
}
