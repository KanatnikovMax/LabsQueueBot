using System.Text;

namespace LabsQueueBot.Core.Helpers;

public static class QueueInfoBuildHelper
{
    private const string MultipleQueueHeader = "Дисциплины твоего курса и твои номера в очередях по ним:";
    private const string NoSubjects = "В твоей группе не добавлено ни одной дисциплины";
    private const string SingleQueueHeader = "Твое место в очереди по дисциплине:\n{0} \u2192 {1}";
    private const string OutOfSubject = "отсутствует";
    private const string Waiting = "в ожидании";
    private const string PositionPattern = "{0}. {1}";
    private const string QueuePositionPattern = "{0}. {1}";
    private const string QueueHeader = "Текущая очередь по дисциплине {0}:";
    private const string WaitingHeader = "Текущая очередь ожидания по дисциплине {0}:";
    private const string QueueWaitingEmpty = " пуста";
    private const string QueueWaitingCount = " {0}";
    
    public static string GetByUser(long userId, Dictionary<string, (List<long> queue, List<long> waiting)> subjects)
    {
        if (subjects.Count == 0)
            return NoSubjects;
        
        var builder = new StringBuilder();
        
        builder.AppendLine(MultipleQueueHeader);
        foreach (var subject in subjects)
        {
            var position = GetPosition(userId, subject.Value.queue, subject.Value.waiting);
            builder.AppendLine(string.Format(PositionPattern, subject.Key, position));
        }

        return builder.ToString();
    }

    public static string GetAllBySubject(string subjectName, List<(string Name, int Idx)> queue, List<string> waiting)
    {
        var builder = new StringBuilder();

        builder.Append(string.Format(QueueHeader, subjectName));
        if (queue.Count == 0)
        {
            builder.AppendLine(QueueWaitingEmpty);
        }
        else
        {
            builder.AppendLine();
            foreach (var userInfo in queue)
                builder.AppendLine(string.Format(PositionPattern, userInfo.Idx + 1, userInfo.Name));
        }

        builder.AppendLine();
        
        builder.Append(string.Format(WaitingHeader, subjectName));
        if (waiting.Count == 0)
        {
            builder.Append(QueueWaitingEmpty);
        }
        else
        {
            builder.AppendLine(string.Format(QueueWaitingCount, waiting.Count));
            
            foreach (var userName in waiting)
                builder.AppendLine(userName);
        }

        return builder.ToString();
    }

    public static string GetSingleBySubject(long userId, string subjectName, List<long> queue, List<long> waiting)
    {
        var position = GetPosition(userId, queue, waiting);

        return string.Format(SingleQueueHeader, subjectName, position);
    }

    private static string GetPosition(long userId, List<long> queue, List<long> waiting)
    {
        var queueIndex = queue.IndexOf(userId);
        if (queueIndex != -1)
            return (queueIndex + 1).ToString();
        
        return waiting.Contains(userId) ? Waiting : OutOfSubject;
    }
}