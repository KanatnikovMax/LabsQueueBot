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

        //random_queue_update
        //ДОБАВИЛ
        // лист-куча для рандомизации основной очереди
        //private readonly List<User> _heap = new();
        private readonly Dictionary<long, byte> _heap = new();
        private byte _maxChance = 0;

        //random_queue_update
        // Count теперь возвращает полное количество студентов: в очереди и ожидающих распределения
        //ДОБАВИЛ
        public int Count { get => _data.Count + _heap.Count; }
        //ВМЕСТО
        //public int Count { get => _data.Count; }

        //random_queue_update
        // position теперь ищет user'а еще и в куче: возвращает 0, если пользователь в куче
        //                                           возвращает -1, если пользователя нет ни в очереди, ни в куче
        //                                           возвращает индекс user'а в очереди + 1 (по сути номер места в очереди),
        //                                                         если пользователь есть в текущей очереди
        // 0 или -index-1, если user в куче? важно для Remove() и Group.FindPosition()
        // Решили -index-1
        // решили на мапе, теперь все менять...
        //ДОБАВИЛ



        public int Position(long id)
        {
            int index = _data.FindIndex(0, _data.Count, value => value == id);
            if (index >= 0)
                return index + 1;
            if (_heap.ContainsKey(id))
                return -1;
            return 0;
        }
        //ВМЕСТО
        //public int Position(long id) => _data.FindIndex(0, Count, val => val.ID == id);

        //random_queue_update
        //
        //Добавил
        public void Add(User user)
        {
            // добавление user'а первоначально в кучу
            _heap.Add(user.ID, 0);
            // заглушка вместо нормального рандома: пока в очереди меньше 2 - первый из кучи добавляется в очереди
            PushHeap();
        }
        //ВМЕСТО
        //public void Add(User user) => _data.Add(user);

        //random_queue_update
        // Clear теперь очищает и очередь, и кучу
        //ДОБАВИЛ
        public void Clear()
        {
            _data.Clear();
            _heap.Clear();
        }
        //ВМЕСТО
        //public void Clear() => _data.Clear();

        //random_queue_update
        // исправил удаление user'a из очереди, теперь удаляет и из heap
        public bool Remove(long id)
        {
            var index = Position(id);
            if (index == 0)
                return false;
            //ДОБАВИЛ
            if (index > 0)
                _data.RemoveAt(index - 1);
            else
                _heap.Remove(id);
            PushHeap();
            //ВМЕСТО
            //_data.RemoveAt(index);
            return true;
        }


        // нигде не используется - мб удалить??
        public void Pop()
        {
            if (Count > 0)
            {
                _data.RemoveAt(0);
            }
        }

        public IEnumerator<long> GetEnumerator() => _data.GetEnumerator();
        

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public User this[int position]
        {
            get => Users.At(_data[position]);
            private set => Users.At(_data[position]) = value;
        }

        public void Skip(long id)
        {
            var index = Position(id);

            if (index < 0)
                throw new InvalidOperationException("Тебя тут нет, кого ты пропускаешь?");

            //random_queue_update
            //ДОБАВИЛ
            // 0 место означает, что user в куче, но не в очереди
            if (index == 0)
                throw new InvalidOperationException("Пропускать некгого - ваше место в очереди еще не определено");
            //

            //random_queue_update
            // тк Position() возвращает место в очереди - достаточно лишь Count, а в индексации необходим -1
            //ДОБАВИЛ
            if (index == Count)
                throw new InvalidOperationException("Ты уже итак в конце очереди, ожидай своего часа :)");
            (_data[index - 1], _data[index]) = (_data[index], _data[index - 1]);
            //ВМЕСТО
            //if (index == Count - 1)
            //    throw new InvalidOperationException("Ты уже итак в конце очереди, ожидай своего часа :)");
            //(_data[index], _data[index + 1]) = (_data[index + 1], _data[index]);

        }

        //random_queue_update
        //ДОБАВИЛ
        // PushHeap() для добавления ожидающих студентов в очередь
        public bool PushHeap()
        {
            RandomNumberGenerator.Create();
            if (_data.Count >= 15)
                return false;
            List<long> waiting = _heap.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
            while (_data.Count < 15 && _heap.Count > 0)
            {
                int index = -1;
                while (index < waiting.Count && _maxChance == _heap[waiting[index + 1]])
                    index++;
                
                if (index != -1)
                {
                    int indexPlusOne = index + 1;
                    List<long> awaiting = waiting[..indexPlusOne];
                    while (index >= 0 && waiting.Count > 0 && _data.Count < 15)
                    {
                        int awaitIndex = RandomNumberGenerator.GetInt32(0, (indexPlusOne));
                        index--;
                        _data.Add(_heap[awaiting[awaitIndex]]);
                        _heap.Remove(awaiting[awaitIndex]);
                        awaiting.RemoveAt(awaitIndex);
                    }
                    if (awaiting.Count > 0)
                        waiting = awaiting.Concat(waiting).ToList();
                    else
                        waiting = waiting[indexPlusOne..];
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
