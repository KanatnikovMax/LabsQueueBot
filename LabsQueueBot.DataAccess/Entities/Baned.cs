namespace LabsQueueBot.DataAccess.Entities;

public class Baned
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public int SubjectId { get; set; }
    public DateTime? UnbanDate { get; set; }
    public long ExecutorId { get; set; }
}