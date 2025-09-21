using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.BusinessLogic.Services;

public interface IUserManagementService
{
    Task DeleteUser(User user, CancellationToken cancellationToken);
}