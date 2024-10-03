namespace LabsQueueBot
{
    /// <summary>
    /// Сущность пользователя, находящегося в очереди по дисциплине; <br/>
    /// хранит в себе Id пользователя, дисциплину и номер в очереди по этой дисциплине
    /// </summary>
    public class SerialNumber
    {
        /// <summary>
        /// Суррогатный ключ - Id пользователя в очереди
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер пользователя в очереди по дисциплине
        /// </summary>
        public int QueueIndex { get; set; }

        /// <summary>
        /// Id пользователя
        /// </summary>
        public long TgUserIndex { get; set; }

        /// <summary>
        /// Реализация связи один ко многим с сущностью Subject
        /// </summary>
        public Subject Subject { get; set; }

        /// <summary>
        /// Внешний ключ Subject
        /// </summary>
        public int SubjectId { get; set; }
    }
}