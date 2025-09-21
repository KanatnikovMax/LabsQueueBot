using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.Repository.Repository;

public interface ISubjectRepository : IRepository<Subject>
{
    Task<Subject?> GetByGroupAndName(byte course, byte group, string subjectName, CancellationToken cancellationToken);
    Task<IEnumerable<Subject>> GetByGroup(byte course, byte group, CancellationToken cancellationToken);
    Task DeleteBatchAsync(IReadOnlyCollection<Subject> entities, CancellationToken cancellationToken);
}