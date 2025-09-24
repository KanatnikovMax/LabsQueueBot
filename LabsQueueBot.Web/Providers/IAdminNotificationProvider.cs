namespace LabsQueueBot.Web.Providers;

public interface IAdminNotificationProvider
{
    Task NotifyAdminsWithDocument(int documentId, string message, CancellationToken cancellationToken);
}