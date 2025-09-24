namespace LabsQueueBot.Web.Providers;

public interface IQueueInfoNotificationProvider
{
    Task NotifyAll(CancellationToken cancellationToken);
    Task NotifyGroup(byte course, byte group, CancellationToken cancellationToken);
    Task NotifyGroupBySubject(byte course, byte group, string subjectName, List<long> queue, List<long> waiting, CancellationToken cancellationToken);
    Task NotifyUserBySubject(long userId, string subjectName, CancellationToken cancellationToken, byte? course = null, byte? group = null);
}