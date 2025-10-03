using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.Repository.Repository;

public interface IBlackListRepository : IRepository<Banned>
{
    Task<Banned?> GetBanByUserAndSubject(long banedUserId, int subjectId, CancellationToken cancellationToken);
}