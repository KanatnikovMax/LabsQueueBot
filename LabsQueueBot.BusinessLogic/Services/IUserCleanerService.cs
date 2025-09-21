using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.BusinessLogic.Services;

public interface IUserCleanerService
{
    Task ClearAll(CancellationToken cancellationToken);
    Task ClearOrDeleteAll(CancellationToken cancellationToken);
}