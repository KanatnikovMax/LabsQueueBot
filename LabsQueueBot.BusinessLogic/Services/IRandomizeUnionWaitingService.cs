using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.BusinessLogic.Services;

public interface IRandomizeUnionWaitingService
{
    Task RandomizeAndUnionWaitingByGroup(byte course, byte group, CancellationToken cancellationToken);
    Task<Subject?> RandomizeAndUnionWaitingBySubject(int subjectId, CancellationToken cancellationToken);

    Task<IEnumerable<Subject>> RandomizeAndUnionWaitingByBatch(IReadOnlyCollection<int> subjectsIds, CancellationToken cancellationToken);
}