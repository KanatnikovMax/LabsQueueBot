namespace LabsQueueBot.Web.Providers;

public interface INotificationProvider
{
    Task NotifyAll(CancellationToken cancellationToken);
    Task NotifyGroup(byte course, byte group, CancellationToken cancellationToken);
    Task NotifyGroupBySubject(byte course, byte group, string subjectName, List<long> queue, List<long> waiting, CancellationToken cancellationToken);
    Task NotifyUserBySubject(long userId, string subjectName, CancellationToken cancellationToken); Task NotifyAdminsWithDocument(int documentId, string message, CancellationToken cancellationToken);
}