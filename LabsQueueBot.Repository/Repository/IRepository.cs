using System.Linq.Expressions;

namespace LabsQueueBot.Repository.Repository;

public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<T> SaveAsync(T entity, CancellationToken cancellationToken);
    Task DeleteAsync(T entity, CancellationToken cancellationToken);
}