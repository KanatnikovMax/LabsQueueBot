using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.Repository.Repository;

public interface IBlackListRepository : IRepository<Baned>
{
    Task<Baned?> GetBanByUserAndSubject(long banedUserId, int subjectId, CancellationToken cancellationToken);
}