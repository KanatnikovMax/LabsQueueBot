namespace LabsQueueBot
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
        public string SubjectName { get; set; }

        /// <summary>
        /// Номер курса, у которого ведется дисциплина
        /// </summary>
        public byte CourseNumber { get; set; }

        /// <summary>
        /// Номер группы, у которой ведется дисциплина
        /// </summary>
        public byte GroupNumber { get; set; }

        /// <summary>
        /// Реализация связи один ко многим с сущностью SerialNumber
        /// </summary>
        public List<SerialNumber> SerialNumbers { get; set; } = new();
    }
}