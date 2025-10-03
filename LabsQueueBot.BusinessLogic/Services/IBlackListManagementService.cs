namespace LabsQueueBot.BusinessLogic.Services;

public interface IBlackListManagementService
{
    Task<DateTime> BanBySubject(long userToBanId, int subjectId, int timeoutInDays, long executorId, CancellationToken cancellationToken);
    Task<bool> UnbanBySubject(long userId, int subjectId, CancellationToken cancellationToken);
    Task UnbanAllByTimeout(CancellationToken cancellationToken);
}