using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace LabsQueueBot
{
    internal class Queue : IEnumerable<long>
    {
        // хранятся ID пользователей
        private readonly List<long> _data = new();
        private readonly Dictionary<long, byte> _heap = new();
        private byte _maxChance = 0;
        public int Count { get => _data.Count + _heap.Count; }


        //  возвращает 0, если пользователь в куче
        //  возвращает -1, если пользователя нет ни в очереди, ни в куче
        //  возвращает индекс user'а в очереди + 1 (по сути номер места в очереди),
        // если пользователь есть в текущей очереди
        public int Position(long id)
        {
            int index = _data.FindIndex(0, _data.Count, value => value == id);
            if (index >= 0)
                return index + 1;
            if (_heap.ContainsKey(id))
                return -1;
            return 0;
        }

        public void Add(User user)
        {
            _heap.Add(user.ID, 0);
            PushHeap();
        }

        public void Clear()
        {
            _data.Clear();
            _heap.Clear();
        }

        // исправил удаление user'a из очереди, теперь удаляет и из heap
        public bool Remove(long id)
        {
            var index = Position(id);
            if (index == 0)
                return false;
            if (index > 0)
                _data.RemoveAt(index - 1);
            else
                _heap.Remove(id);
            PushHeap();
            return true;
        }

        public IEnumerator<long> GetEnumerator() => _data.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Skip(long id)
        {
            var index = Position(id);

            if (index < 0)
                throw new InvalidOperationException("Тебя тут нет, кого ты пропускаешь?");

            if (index == 0)
                throw new InvalidOperationException(
                    "Пропускать некого - ваше место в очереди еще не определено");

            if (index == Count)
                throw new InvalidOperationException(
                    "Ты уже итак в конце очереди, ожидай своего часа :)");

            (_data[index - 1], _data[index]) = (_data[index], _data[index - 1]);

        }

        // PushHeap() для добавления ожидающих студентов в очередь
        public bool PushHeap()
        {
            if (_data.Count >= 15)
                return false;
            List<long> waiting = _heap.OrderByDescending(x => x.Value)
                .Select(x => x.Key).ToList();
            while (_data.Count < 15 && _heap.Count > 0)
            {
                int index = -1;
                while (index < _heap.Count - 1 && index < waiting.Count 
                    && _maxChance == _heap[waiting[index + 1]])
                    index++;
                
                if (index != -1)
                {
                    int indexPlusOne = index + 1;
                    List<long> awaiting = waiting.GetRange(0, indexPlusOne);
                    while (index >= 0 && waiting.Count > 0 && _data.Count < 15)
                    {
                        int awaitIndex = RandomNumberGenerator.GetInt32(0, indexPlusOne);
                        index--;
                        _data.Add(awaiting[awaitIndex]);
                        _heap.Remove(awaiting[awaitIndex]);
                        awaiting.RemoveAt(awaitIndex);
                    }
                    if (awaiting.Count > 0)
                        waiting = awaiting.Concat(waiting).ToList();
                    else
                        waiting.RemoveRange(0, indexPlusOne);
                }
                if (_data.Count < 15 && waiting.Count > 0)
                    _maxChance--;

            }
            return true;
        }
        //

        public void UpdateHeap()
        {

        }
    }
}
