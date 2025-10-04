using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Repository.Repository;

namespace LabsQueueBot.BusinessLogic.Services.Implementation;

public class SubjectsManagementService(ISubjectRepository subjectRepository) : ISubjectsManagementService
{
    public async Task<IReadOnlyCollection<Subject>> GetByDayOfWeekInTimetable(WeekDays dayOfWeek, bool isNumWeek, CancellationToken cancellationToken)
    {
        var subjects = (await subjectRepository.GetByConditionAsync(
                x => isNumWeek
                    ? WeekDaysHelper.ParseWeekDays((WeekDays)x.NumWeekTimetableMask).Contains(dayOfWeek)
                    : WeekDaysHelper.ParseWeekDays((WeekDays)x.DenWeekTimetableMask).Contains(dayOfWeek), 
                cancellationToken))
            .ToList();

        return subjects;
    }
    
    public async Task DeleteUserFromSubjectsQueues(long userId, byte course, byte group, CancellationToken cancellationToken)
    {
        var subjects = (await subjectRepository.GetByGroup(course, group, cancellationToken)).ToList();
        
        subjects = RemoveUserFromQueueWaiting(subjects, userId);

        await DeleteEmptySubjects(subjects, cancellationToken);
    }
    
    private async Task DeleteEmptySubjects(List<Subject> subjects, CancellationToken cancellationToken)
    {
        var subjectsToDelete = subjects
            .Where(s => s.Queue.Length == 0 && s.Waiting.Length == 0)
            .ToList();
        if (subjectsToDelete.Count != 0)
            await subjectRepository.DeleteBatchAsync(subjectsToDelete, cancellationToken);

        var subjectsToUpdate = subjects.Except(subjectsToDelete).ToList();
        await subjectRepository.UpdateBatchAsync(subjectsToUpdate, cancellationToken);
    }
    
    private static List<Subject> RemoveUserFromQueueWaiting(List<Subject> subjects, long userId)
    {
        foreach (var subject in subjects)
        {
            if (subject.Queue.Contains(userId))
                subject.Queue = subject.Queue.Where(x => x != userId).ToArray();
            if (subject.Waiting.Contains(userId))
                subject.Waiting = subject.Waiting.Where(x => x != userId).ToArray();
        }

        return subjects;
    }
}