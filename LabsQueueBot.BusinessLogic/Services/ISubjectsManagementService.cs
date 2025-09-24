namespace LabsQueueBot.BusinessLogic.Services;

public interface ISubjectsManagementService
{
    Task DeleteUserFromSubjectsQueues(long userId, byte course, byte group, CancellationToken cancellationToken);
}