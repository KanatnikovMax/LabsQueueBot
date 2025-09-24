using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.BusinessLogic.Services;

public interface IUserManagementService
{
    Task PutUserIntoGroup(User user, byte course, byte group, CancellationToken cancellationToken);
}