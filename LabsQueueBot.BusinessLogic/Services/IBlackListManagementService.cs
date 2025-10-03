namespace LabsQueueBot.BusinessLogic.Services;

public interface IBlackListManagementService
{
    Task<DateTime> BanUserBySubject(long userToBanId, int subjectId, int timeoutInDays, long executorId, CancellationToken cancellationToken);
    Task<bool> UnbanUserBySubject(long userId, int subjectId, CancellationToken cancellationToken);
}