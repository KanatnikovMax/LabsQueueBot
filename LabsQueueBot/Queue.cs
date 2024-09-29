using System.Collections;
using System.Security.Cryptography;

namespace LabsQueueBot
{

    public class Queue : IEnumerable<long>
    {
        public List<long> _queue = new(30);
        public List<long> _waiting = new(30);
        private readonly int _subjectId;
        public Queue(int subjectId) => _subjectId = subjectId;
        public Queue(byte courseNumber, byte groupNumber, string subjectName, int subjectId)
        {
            CourseNumber = courseNumber;
            GroupNumber = groupNumber;
            SubjectName = subjectName;
            _subjectId = subjectId;
            _indexLast = 0;
            using (var db = new QueueBotContext())
            {
                var snq = db.SerialNumberRepository
                    .Where(sn => sn.SubjectId == subjectId)
                    .OrderBy(sn => sn.QueueIndex);
                if (snq.Count() > 0 ) 
                    _indexLast = snq.Last().QueueIndex;
                _queue = snq.Select(sn => sn.TgUserIndex).ToList();
            }
        }

        public string SubjectName { get; set; }
        public byte CourseNumber { get; set; }
        public byte GroupNumber { get; set; }

        public int Count { get => _queue.Count + _waiting.Count; }

        private int _indexLast;

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

        public void Add(User user)
        {
            _waiting.Add(user.Id);
        }

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
                using (var db = new QueueBotContext())
                {
                    var sbToRemove = db.SerialNumberRepository
                        .FirstOrDefault(s => s.TgUserIndex == id
                        && s.SubjectId == _subjectId);
                    db.SerialNumberRepository.Remove(sbToRemove);
                    db.SaveChanges();                   
                }
                _queue.RemoveAt(index);
                return true;
            }
            
            index = _waiting.FindIndex(0, _waiting.Count, val => val == id);
            if (index >= 0)
            {
                _waiting.RemoveAt(index);
                return true;
            }
            
            return false;
        }

        public void Union()
        {
            using var db = new QueueBotContext();
            var list = new List<SerialNumber>();
            var subject = db.SubjectRepository.Find(_subjectId);
            while (_waiting.Count > 0)
            {
                ++_indexLast;
                int index = RandomNumberGenerator.GetInt32(0, _waiting.Count);
                var userId = _waiting[index];
                _queue.Add(userId);
                var serialNumber = new SerialNumber()
                {
                    QueueIndex = _indexLast,
                    TgUserIndex = userId,
                    SubjectId = _subjectId,
                    Subject = subject
                };
                list.Add(serialNumber);
                _waiting.RemoveAt(index);
            }
            db.SerialNumberRepository.AddRange(list);
            db.SaveChanges();
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

            using var db = new QueueBotContext();
            var sn1 = db.SerialNumberRepository
                .FirstOrDefault(sn => sn.TgUserIndex == _queue[index]
                && sn.SubjectId == _subjectId);
            var sn2 = db.SerialNumberRepository
                .FirstOrDefault(sn => sn.TgUserIndex == _queue[index + 1]
                && sn.SubjectId == _subjectId);
            (sn1.QueueIndex, sn2.QueueIndex) = (sn2.QueueIndex, sn1.QueueIndex);
            db.SerialNumberRepository.UpdateRange([sn1, sn2]);
            db.SaveChanges();
        }
    }
}
