using LabsQueueBot.Core.Enums;
using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.BusinessLogic.Services;

public interface ISubjectsManagementService
{
    Task DeleteUserFromSubjectsQueues(long userId, byte course, byte group, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Subject>> GetByDayOfWeekInTimetable(WeekDays dayOfWeek, bool isNumWeek, CancellationToken cancellationToken);
}