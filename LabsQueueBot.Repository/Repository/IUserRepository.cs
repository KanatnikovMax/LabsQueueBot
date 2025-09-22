using System.Linq.Expressions;
using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.Repository.Repository;

public interface IUserRepository : IRepository<User>
{
    Task<IEnumerable<(byte course, byte group)>> GetAllGroups(CancellationToken cancellationToken);
    Task<IEnumerable<User>> GetGroup(byte course, byte group, CancellationToken cancellationToken);
    Task DeleteByConditionAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);
}