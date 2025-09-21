using System.Linq.Expressions;
using LabsQueueBot.DataAccess;
using LabsQueueBot.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LabsQueueBot.Repository.Repository.Implements;

public class UserRepository(IDbContextFactory<QueueBotContext> contextFactory) : IUserRepository
{
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<User>().AsNoTracking().ToListAsync(cancellationToken);
    }


    public async Task<IEnumerable<User>> GetByConditionAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<User>().AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<User>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
    
    public async Task<User> SaveAsync(User entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        EntityEntry<User> result;
        
        if (await dbContext.Set<User>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken) == null)
        {
            result = await dbContext.Set<User>().AddAsync(entity, cancellationToken);
        }
        else
        {
            result = dbContext.Set<User>().Update(entity);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return result.Entity;
    }

    public async Task UpdateBatchAsync(IReadOnlyCollection<User> entities, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        
        dbContext.Set<User>().UpdateRange(entities);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteAsync(User entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Set<User>().Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteByConditionAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entities = dbContext.Set<User>().Where(predicate);
        dbContext.Set<User>().RemoveRange(entities);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<(byte, byte)>> GetAllGroups(CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return (await dbContext.Set<User>()
            .AsNoTracking()
            .Where(u => u.CourseNumber != 0 && u.GroupNumber != 0)
            .Select(u => new KeyValuePair<byte, byte>(u.CourseNumber, u.GroupNumber))
            .Distinct()
            .ToListAsync(cancellationToken))
            .Select(kv => (kv.Key, kv.Value));
    }
}