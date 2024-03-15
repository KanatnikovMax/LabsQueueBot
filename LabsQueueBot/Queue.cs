using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LabsQueueBot
{
    internal class Queue : IEnumerable<User>
    {
        private readonly List<User> _data = new();
        public User First { get => _data[0]; }
        public int Count { get => _data.Count; }
        public int Position(long id) => _data.FindIndex(0, Count, val => val.ID == id);
        public void Add(User user) => _data.Add(user);
        public void Clear() => _data.Clear();
        public bool Remove(long id)
        {
            var index = Position(id);
            if (index < 0)
                return false;
            //_data[index].State = Waiting;
            _data.RemoveAt(index);
            return true;
        }
        public void Pop()
        {
            if (Count > 0)
            {
                //_data[0].State = Waiting;
                _data.RemoveAt(0);
            }
        }

        public IEnumerator<User> GetEnumerator() => _data.GetEnumerator();
        

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public User this[int position]
        {
            get => _data[position];
            private set => _data[position] = value;
        }

        public bool Skip(long id)
        {
            var index = Position(id);
            if (index < 0)
                return false;
            if (index == Count - 1)
                throw new ArgumentOutOfRangeException("Ты уже итак в конце очереди, ожидай своего часа :)");
            (_data[index], _data[index + 1]) = (_data[index + 1], _data[index]);
            return true;
        }


        
        //public readonly byte Group;
        
    }
}
