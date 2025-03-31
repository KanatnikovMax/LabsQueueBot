using System.Linq.Expressions;
using LabsQueueBot.Db;
using LabsQueueBot.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LabsQueueBot.Bot;

public class SerialNumberRepository : IRepository<SerialNumber>
{
    private readonly IDbContextFactory<QueueBotContext> _contextFactory;
    
    public async Task<IEnumerable<SerialNumber>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<SerialNumber>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SerialNumber>> GetAllAsync(Expression<Func<SerialNumber, bool>> predicate, CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<SerialNumber>().AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<SerialNumber?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<SerialNumber>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<SerialNumber> SaveAsync(SerialNumber entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        EntityEntry<SerialNumber> result;
        
        if (await dbContext.Set<SerialNumber>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken) == null)
        {
            result = await dbContext.Set<SerialNumber>().AddAsync(entity, cancellationToken);
        }
        else
        {
            result = dbContext.Set<SerialNumber>().Update(entity);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return result.Entity;
    }

    public async Task DeleteAsync(SerialNumber entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Set<SerialNumber>().Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<SerialNumber>> GetQueueBySubject(Subject subject, CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<SerialNumber>()
            .AsNoTracking()
            .Where(sn => sn.SubjectId == subject.Id && sn.QueueIndex != -1)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SerialNumber>> GetWaitingBySybject(Subject subject, CancellationToken cancellationToken)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<SerialNumber>()
            .AsNoTracking()
            .Where(sn => sn.SubjectId == subject.Id && sn.QueueIndex == -1)
            .ToListAsync(cancellationToken);
    }
}