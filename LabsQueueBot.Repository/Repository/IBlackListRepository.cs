using System.Linq.Expressions;
using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.Repository.Repository;

public interface IBlackListRepository : IRepository<Banned>
{
    Task<Banned?> GetBanByUserAndSubject(long banedUserId, int subjectId, CancellationToken cancellationToken);
    Task DeleteByConditionAsync(Expression<Func<Banned, bool>> predicate, CancellationToken cancellationToken);
}