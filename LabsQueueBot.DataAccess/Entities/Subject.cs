using System.ComponentModel.DataAnnotations;
using LabsQueueBot.Core.Enums;

namespace LabsQueueBot.DataAccess.Entities
{
    /// <summary>
    /// Сущность дисциплины для хранения в БД;
    /// хранит в себе данные о дисциплине: название, курс и группу у которой ведется
    /// </summary>
    public class Subject
    {
        /// <summary>
        /// Суррогатный ключ - Id дисциплины
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название дисциплины
        /// </summary>
        [MaxLength(50)]
        public string SubjectName { get; set; } = string.Empty;

        /// <summary>
        /// Номер курса, у которого ведется дисциплина
        /// </summary>
        public byte CourseNumber { get; set; }

        /// <summary>
        /// Номер группы, у которой ведется дисциплина
        /// </summary>
        public byte GroupNumber { get; set; }
        
        /// <summary>
        /// Маска дней недели в числитель
        /// </summary>
        public int NumWeekTimetableMask { get; set; } = (int) WeekDays.All;
    
        /// <summary>
        /// Маска дней недели в знаменатель
        /// </summary>
        public int DenWeekTimetableMask { get; set; } = (int) WeekDays.All;

        public long[] Queue { get; set; } = [];
        public long[] Waiting { get; set; } = [];
    }
}