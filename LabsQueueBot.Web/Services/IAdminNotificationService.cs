namespace LabsQueueBot.Web.Services;

public interface IAdminNotificationService
{
    Task NotifyWithDocument(int documentId, string message, CancellationToken cancellationToken);
    Task NotifyWithMessage(string message, CancellationToken cancellationToken);
}