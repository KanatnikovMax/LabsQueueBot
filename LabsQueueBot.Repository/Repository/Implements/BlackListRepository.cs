using System.Linq.Expressions;
using LabsQueueBot.DataAccess;
using LabsQueueBot.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LabsQueueBot.Repository.Repository.Implements;

public class BlackListRepository(IDbContextFactory<QueueBotContext> contextFactory) : IBlackListRepository
{
    public async Task<IEnumerable<Baned>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Baned>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Baned>> GetByConditionAsync(Expression<Func<Baned, bool>> predicate, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Baned>().AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Baned?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Baned>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Baned> SaveAsync(Baned entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        EntityEntry<Baned> result;

        if (await dbContext.Set<Baned>().AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken) == null)
        {
            result = await dbContext.Set<Baned>().AddAsync(entity, cancellationToken);
        }
        else
        {
            result = dbContext.Set<Baned>().Update(entity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return result.Entity;
    }

    public async Task UpdateBatchAsync(IReadOnlyCollection<Baned> entities, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        
        dbContext.Set<Baned>().UpdateRange(entities);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Baned entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Set<Baned>().Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<Baned?> GetBanByUserAndSubject(long banedUserId, int subjectId, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Baned>().AsNoTracking().FirstOrDefaultAsync(e => e.UserId == banedUserId && e.SubjectId == subjectId, cancellationToken);
    }
}