using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Repository.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace LabsQueueBot.BusinessLogic.Services.Implementation;

public class UserManagementService(
    IServiceScopeFactory scopeFactory) : IUserManagementService
{
    public async Task DeleteUser(User user, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var subjectRepository = scope.ServiceProvider.GetRequiredService<ISubjectRepository>();
        
        var subjects = (await subjectRepository.GetByGroup(user.CourseNumber, user.GroupNumber, cancellationToken)).ToList();
        foreach (var subject in subjects)
        {
            if (subject.Queue.Contains(user.Id))
                subject.Queue = subject.Queue.Where(x => x != user.Id).ToArray();
            if (subject.Waiting.Contains(user.Id))
                subject.Waiting = subject.Waiting.Where(x => x != user.Id).ToArray();
        }

        var subjectsToDelete = subjects
            .Where(s => s.Queue.Length == 0 && s.Waiting.Length == 0)
            .ToList();
        if (subjectsToDelete.Count != 0)
            await subjectRepository.DeleteBatchAsync(subjectsToDelete, cancellationToken);
        
        await userRepository.DeleteAsync(user, cancellationToken);
    }
}