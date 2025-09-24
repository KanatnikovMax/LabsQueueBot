using System.Security.Cryptography;
using LabsQueueBot.Repository.Repository;

namespace LabsQueueBot.BusinessLogic.Services.Implementation;

public class RandomizeUnionWaitingService(
    ISubjectRepository subjectRepository) : IRandomizeUnionWaitingService
{
    public async Task RandomizeAndUnionWaitingByGroup(byte course, byte group, CancellationToken cancellationToken)
    {
        var subjects = (await subjectRepository.GetByConditionAsync(
                x => x.CourseNumber == course && x.GroupNumber == group,
                cancellationToken))
            .ToList();

        foreach (var subject in subjects)
        {
            var randomizedWaiting = RandomizeWaiting(subject.Waiting.ToList());
        
            subject.Queue = subject.Queue.Concat(randomizedWaiting).ToArray();
            subject.Waiting = [];
        }

        await subjectRepository.UpdateBatchAsync(subjects, cancellationToken);
    }

    public async Task RandomizeAndUnionWaitingBySubject(int subjectId, CancellationToken cancellationToken)
    {
        var subject = await subjectRepository.GetByIdAsync(subjectId, cancellationToken);
        if (subject == null)
            return;
        
        var randomizedWaiting = RandomizeWaiting(subject.Waiting.ToList());
        
        subject.Queue = subject.Queue.Concat(randomizedWaiting).ToArray();
        subject.Waiting = [];
        
        await subjectRepository.SaveAsync(subject, cancellationToken);
    }
    
    private static List<long> RandomizeWaiting(List<long> waiting)
    {
        var result = new List<long>();

        while (waiting.Count != 0)
        {
            var index = RandomNumberGenerator.GetInt32(0, waiting.Count);
            result.Add(waiting[index]);
            waiting.RemoveAt(index);
        }

        return result;
    }
}