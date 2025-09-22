namespace LabsQueueBot.BusinessLogic.Services;

public interface ISubjectManageService
{
    Task RandomizeAndUnionWaitingByGroup(byte course, byte group, CancellationToken cancellationToken);
    Task RandomizeAndUnionWaitingBySubject(int subjectId, CancellationToken cancellationToken);
}