using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Repository.Repository;

namespace LabsQueueBot.BusinessLogic.Services.Implementation;

public class BlackListManagementService(
    IBlackListRepository blackListRepository,
    ISubjectRepository subjectRepository) : IBlackListManagementService
{
    public async Task<DateTime> BanUserBySubject(long userToBanId, int subjectId, int timeoutInDays, long executorId, CancellationToken cancellationToken)
    {
        var banned = await blackListRepository.GetBanByUserAndSubject(userToBanId, subjectId, cancellationToken);
        if (banned != null && banned.UnbanDate > DateTime.UtcNow)
        {
            return banned.UnbanDate;
        }

        var subject = await subjectRepository.GetByIdAsync(subjectId, cancellationToken);
        if (subject == null)
        {
            throw new ArgumentNullException($"SubjectId <{subjectId}> указывает на несуществующую дисциплину"); 
        }
        
        // удаляем пользователя из очереди ожидания
        subject.Waiting = subject.Waiting.Where(x => x != userToBanId).ToArray();

        // обновляем информацию о бане
        banned ??= new Banned
        {
            UserId = userToBanId,
            SubjectId = subject.Id
        };
        banned.ExecutorId = executorId;
        banned.UnbanDate = DateTime.UtcNow + TimeSpan.FromDays(timeoutInDays);

        Task.WaitAll([
            subjectRepository.SaveAsync(subject, cancellationToken),
            blackListRepository.SaveAsync(banned, cancellationToken)
        ], cancellationToken);

        return banned.UnbanDate;
    }

    public async Task<bool> UnbanUserBySubject(long userId, int subjectId, CancellationToken cancellationToken)
    {
        var banned = await blackListRepository.GetBanByUserAndSubject(userId, subjectId, cancellationToken);
        if (banned == null || banned.UnbanDate < DateTime.UtcNow)
        {
            return false;
        }
        
        banned.UnbanDate = DateTime.UtcNow;
        await blackListRepository.SaveAsync(banned, cancellationToken);

        return true;
    }
}