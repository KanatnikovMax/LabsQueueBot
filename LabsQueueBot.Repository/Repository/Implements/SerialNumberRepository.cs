using System.Linq.Expressions;
using LabsQueueBot.DataAccess;
using LabsQueueBot.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LabsQueueBot.Repository.Repository.Implements;

public class SerialNumberRepository(IDbContextFactory<QueueBotContext> contextFactory) : ISerialNumberRepository
{
    public async Task<IEnumerable<SerialNumber>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<SerialNumber>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SerialNumber>> GetByConditionAsync(Expression<Func<SerialNumber, bool>> predicate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<SerialNumber>().AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<SerialNumber?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<SerialNumber>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<SerialNumber> SaveAsync(SerialNumber entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
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
    
    public async Task UpdateBatchAsync(IReadOnlyCollection<SerialNumber> entities, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        
        dbContext.Set<SerialNumber>().UpdateRange(entities);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(SerialNumber entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Set<SerialNumber>().Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<SerialNumber>> GetQueueBySubject(Subject subject, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<SerialNumber>()
            .AsNoTracking()
            .Where(sn => sn.SubjectId == subject.Id && sn.QueueIndex != -2)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SerialNumber>> GetWaitingBySubject(Subject subject, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<SerialNumber>()
            .AsNoTracking()
            .Where(sn => sn.SubjectId == subject.Id && sn.QueueIndex == -2)
            .ToListAsync(cancellationToken);
    }

    public async Task SwapUsersInQueue(SerialNumber sn1, SerialNumber sn2, CancellationToken cancellationToken) // TODO проверить работоспособность
    {
        throw new NotImplementedException();
        
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        (sn1.QueueIndex, sn2.QueueIndex) = (sn2.QueueIndex, sn1.QueueIndex);
        dbContext.Set<SerialNumber>().Update(sn1);
        dbContext.Set<SerialNumber>().Update(sn2);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveRange(IEnumerable<SerialNumber> range, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        // await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);
        // dbContext.SerialNumberRepository.UpdateRange(range);
        // await dbContext.SaveChangesAsync(cancellationToken);
    }
}