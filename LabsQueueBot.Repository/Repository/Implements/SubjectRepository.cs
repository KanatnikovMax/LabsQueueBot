using System.Linq.Expressions;
using LabsQueueBot.DataAccess;
using LabsQueueBot.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LabsQueueBot.Repository.Repository.Implements;

public class SubjectRepository(IDbContextFactory<QueueBotContext> contextFactory) : ISubjectRepository
{
    public async Task<IEnumerable<Subject>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Subject>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subject>> GetByConditionAsync(Expression<Func<Subject, bool>> predicate,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Subject>().AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Subject?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Subject>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Subject> SaveAsync(Subject entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        EntityEntry<Subject> result;

        if (await dbContext.Set<Subject>().AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken) == null)
        {
            result = await dbContext.Set<Subject>().AddAsync(entity, cancellationToken);
        }
        else
        {
            result = dbContext.Set<Subject>().Update(entity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return result.Entity;
    }
    
    public async Task UpdateBatchAsync(IReadOnlyCollection<Subject> entities, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        
        dbContext.Set<Subject>().UpdateRange(entities);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Subject entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Set<Subject>().Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteBatchAsync(IReadOnlyCollection<Subject> entities, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Set<Subject>().RemoveRange(entities);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Subject?> GetByGroupAndName(byte course, byte group, string subjectName,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<Subject>().AsNoTracking().FirstOrDefaultAsync(e =>
                e.CourseNumber == course
                && e.GroupNumber == group
                && e.SubjectName == subjectName,
            cancellationToken);
    }

    public async Task<IEnumerable<Subject>> GetByGroup(byte course, byte group, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return dbContext.Set<Subject>()
            .AsNoTracking()
            .Where(e => e.CourseNumber == course && e.GroupNumber == group)
            .ToList();
    }
}