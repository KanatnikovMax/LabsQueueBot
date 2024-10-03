using System.Collections;
using System.Security.Cryptography;

namespace LabsQueueBot
{
    /// <summary>
    /// Очередь пользователей; <br/>
    /// представляет из себя два списка: <br/>
    /// - список ожидающих распределения пользователей; <br/>
    /// - список распределенных в очереди пользователей
    /// </summary>
    public class Queue : IEnumerable<long>
    {
        /// <summary>
        /// Список распределенных в очереди пользователей
        /// </summary>
        public List<long> _queue = new(30);

        /// <summary>
        /// Список ожидающих распределения пользователей
        /// </summary>
        public List<long> _waiting = new(30);

        /// <summary>
        /// Внешний ключ - Id сущности Subject <br/>
        /// (для работы с БД)
        /// </summary>
        private readonly int _subjectId;

        /// <summary>
        /// Конструктор класса Queue
        /// </summary>
        /// <param name="subjectId"> внешний ключ к сущности Subject </param>
        public Queue(int subjectId) => _subjectId = subjectId;

        /// <summary>
        /// Конструктор класса Queue; <br/>
        /// синхронизирует данные очереди из БД
        /// </summary>
        /// <param name="courseNumber"> номер курса </param>
        /// <param name="groupNumber"> номер группы </param>
        /// <param name="subjectName"> название дисципллины </param>
        /// <param name="subjectId"> внешний ключ к сущности Subject </param>
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
                if (snq.Count() > 0)
                    _indexLast = snq.Last().QueueIndex;
                _queue = snq.Where(sn => sn.QueueIndex != -2).Select(sn => sn.TgUserIndex).ToList();
                _waiting = snq.Where(sn => sn.QueueIndex == -2).Select(sn => sn.TgUserIndex).ToList();
            }
        }

        public string SubjectName { get; set; }

        /// <summary>
        /// Номер курса
        /// </summary>
        public byte CourseNumber { get; set; }

        /// <summary>
        /// Номер группы
        /// </summary>
        public byte GroupNumber { get; set; }

        /// <summary>
        /// Количество пользователей в очереди и ожидающих распределения
        /// </summary>
        public int Count => _queue.Count + _waiting.Count;

        /// <summary>
        /// Индекс последнего пользователя, добавленного в БД
        /// </summary>
        private int _indexLast;

        /// <summary>
        /// Возвращает позицию пользователя в очереди
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        /// <returns>
        /// позицию пользователя в очереди; <br/>
        /// -1, если пользователь не найден в очереди и списке ожидания; <br/>
        /// -2, если пользователь находится в списке ожидания
        /// </returns>
        public int Position(long id)
        {
            var index = _queue.FindIndex(0, _queue.Count, val => val == id);
            return index >= 0
                ? index
                : (_waiting.FindIndex(0, _waiting.Count, val => val == id) >= 0 ? -2 : -1);
        }

        /// <summary>
        /// Добавляет пользователя в список ожидания распределения в очередь; <br/>
        /// обновляет БД
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        public void Add(long id)
        {
            _waiting.Add(id);
            using var db = new QueueBotContext();
            var serialNumber = new SerialNumber()
            {
                QueueIndex = -2,
                TgUserIndex = id,
                SubjectId = _subjectId,
            };
            db.SerialNumberRepository.Add(serialNumber);
            db.SaveChanges();
        }

        /// <summary>
        /// Удаляет пользователя из очереди или списка ожидания; <br/>
        /// обновляет БД
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        /// <returns>
        /// true, если пользователь был удален; <br/>
        /// false, если пользователь не был удален
        /// </returns>
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
                using (var db = new QueueBotContext())
                {
                    var sbToRemove = db.SerialNumberRepository
                        .FirstOrDefault(s => s.TgUserIndex == id
                                             && s.SubjectId == _subjectId);
                    db.SerialNumberRepository.Remove(sbToRemove);
                    db.SaveChanges();
                }

                _waiting.RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Объединяет очередь со списком ожидающих распределения; <br/>
        /// распределяет пользователей из списка ожидания случайным образом
        /// и добавляет их в конец текущей очереди; <br/>
        /// обновляет БД
        /// </summary>
        public void Union()
        {
            using var db = new QueueBotContext();
            var list = new List<SerialNumber>();
            while (_waiting.Count > 0)
            {
                ++_indexLast;
                int index = RandomNumberGenerator.GetInt32(0, _waiting.Count);
                var userId = _waiting[index];
                _queue.Add(userId);
                var serialNumber = db.SerialNumberRepository
                    .FirstOrDefault(sn => sn.TgUserIndex == userId
                                          && sn.SubjectId == _subjectId);
                serialNumber.QueueIndex = _indexLast;
                list.Add(serialNumber);
                _waiting.RemoveAt(index);
            }

            db.SerialNumberRepository.UpdateRange(list);
            db.SaveChanges();
        }

        /// <summary>
        /// Реализует GetEnumerator для класса Queue
        /// </summary>
        public IEnumerator<long> GetEnumerator() => _queue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Меняет местами пользователей в очереди; <br/>
        /// обновляет БД
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        /// <exception cref="InvalidOperationException"> <br/>
        /// если пользователя нет в очереди и списке ожидания; <br/>
        /// если пользователь в списке ожидания; <br/>
        /// если пользователь последний в очереди
        /// </exception>
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