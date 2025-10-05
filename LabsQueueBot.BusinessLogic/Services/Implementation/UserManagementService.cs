using LabsQueueBot.Core.Enums;
using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Repository.Repository;

namespace LabsQueueBot.BusinessLogic.Services.Implementation;

public class UserManagementService(IUserRepository userRepository) : IUserManagementService
{
    public async Task PutUserIntoGroup(User user, byte course, byte group, CancellationToken cancellationToken)
    {
        user.CourseNumber = course;
        user.GroupNumber = group;
        await userRepository.SaveAsync(user, cancellationToken);
    }
}