using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.BusinessLogic.Services;

public interface IUserStateCleanerService
{
    Task ClearAll(CancellationToken cancellationToken);
    Task ClearOrDeleteAll(CancellationToken cancellationToken);
}