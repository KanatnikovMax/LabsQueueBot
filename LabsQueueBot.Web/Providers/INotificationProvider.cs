namespace LabsQueueBot.Web.Providers;

public interface INotificationProvider
{
    Task NotifyAll(CancellationToken cancellationToken);
    Task NotifyGroup(short course, short group, CancellationToken cancellationToken);
    Task NotifyGroupBySubject(short course, short group, string subjectName, List<long> queue, List<long> waiting, CancellationToken cancellationToken);
    Task NotifyAdminsWithDocument(int documentId, string message, CancellationToken cancellationToken);
}