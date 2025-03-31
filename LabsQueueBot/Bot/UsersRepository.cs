using System.Linq.Expressions;
using LabsQueueBot.Db;
using LabsQueueBot.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LabsQueueBot.Bot;

public class UsersRepository : IRepository<User>
{
    private readonly IDbContextFactory<QueueBotContext> _contextFactory;

    public UsersRepository(IDbContextFactory<QueueBotContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<User>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<User>().AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<User>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
    
    public async Task<User> SaveAsync(User entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
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
    
    public async Task DeleteAsync(User entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Set<User>().Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}