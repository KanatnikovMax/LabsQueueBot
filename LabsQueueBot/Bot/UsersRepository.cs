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
    
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.Set<User>().AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync(Expression<Func<User, bool>> predicate)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.Set<User>().AsNoTracking().Where(predicate).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.Set<User>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    }
    
    public async Task<User> SaveAsync(User entity)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        EntityEntry<User> result;
        
        if (await dbContext.Set<User>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id) == null)
        {
            result = await dbContext.Set<User>().AddAsync(entity);
        }
        else
        {
            result = dbContext.Set<User>().Update(entity);
        }
        await dbContext.SaveChangesAsync();
        
        return result.Entity;
    }
    
    public async Task DeleteAsync(User entity)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        dbContext.Set<User>().Remove(entity);
        await dbContext.SaveChangesAsync();
    }
}