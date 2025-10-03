using System.Linq.Expressions;
using LabsQueueBot.DataAccess;
using LabsQueueBot.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LabsQueueBot.Repository.Repository.Implements;

public class BlackListRepository(IDbContextFactory<QueueBotContext> contextFactory) : IBlackListRepository
{
    public async Task<IEnumerable<Banned>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Banned>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Banned>> GetByConditionAsync(Expression<Func<Banned, bool>> predicate, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Banned>().AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Banned?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Banned>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Banned> SaveAsync(Banned entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        EntityEntry<Banned> result;

        if (await dbContext.Set<Banned>().AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken) == null)
        {
            result = await dbContext.Set<Banned>().AddAsync(entity, cancellationToken);
        }
        else
        {
            result = dbContext.Set<Banned>().Update(entity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return result.Entity;
    }

    public async Task UpdateBatchAsync(IReadOnlyCollection<Banned> entities, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        
        dbContext.Set<Banned>().UpdateRange(entities);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Banned entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Set<Banned>().Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<Banned?> GetBanByUserAndSubject(long banedUserId, int subjectId, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Banned>().AsNoTracking().FirstOrDefaultAsync(e => e.UserId == banedUserId && e.SubjectId == subjectId, cancellationToken);
    }
}