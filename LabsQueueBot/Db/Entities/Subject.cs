namespace LabsQueueBot
{
    public class Subject
    {
        public int Id { get; set; } 
        public string SubjectName { get; set; }
        public byte CourseNumber { get; set; }
        public byte GroupNumber { get; set; }
        public List<SerialNumber> SerialNumbers { get; set; } = new();
    }
}
