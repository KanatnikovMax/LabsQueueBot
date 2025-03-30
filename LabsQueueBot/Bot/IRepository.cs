using System.Linq.Expressions;

namespace LabsQueueBot.Bot;

public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(long id);
    Task<T> SaveAsync(T entity);
    Task DeleteAsync(T entity);
}