namespace LabsQueueBot.BusinessLogic.Services;

public interface IRandomizeUnionWaitingService
{
    Task RandomizeAndUnionWaitingByGroup(byte course, byte group, CancellationToken cancellationToken);
    Task RandomizeAndUnionWaitingBySubject(int subjectId, CancellationToken cancellationToken);
}