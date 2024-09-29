namespace LabsQueueBot
{
    public class SerialNumber
    {
        public int Id { get; set; }
        public int QueueIndex { get; set; }
        public long TgUserIndex { get; set; }
        public Subject Subject { get; set; }
        public int SubjectId { get; set; }
    }
}
