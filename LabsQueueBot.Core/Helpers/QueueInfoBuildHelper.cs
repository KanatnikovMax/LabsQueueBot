using System.Text;

namespace LabsQueueBot.Core.Helpers;

public static class QueueInfoBuildHelper
{
    private const string QueueHeader = "Дисциплины твоего курса и твои номера в очередях по ним:";
    private const string NoSubjects = "В твоей группе не добавлено ни одной дисциплины";
    private const string SingleQueueHeader = "Твое место в очереди по дисциплине:\n{0} \u2192 {1}";
    private const string OutOfSubject = "отсутствует";
    private const string Waiting = "в ожидании";
    private const string PositionPattern = "{0} \u2192 {1}";
    
    public static string GetByUser(long userId, Dictionary<string, (List<long> queue, List<long> waiting)> subjects)
    {
        if (subjects.Count == 0)
            return NoSubjects;
        
        var builder = new StringBuilder();
        
        builder.AppendLine(QueueHeader);
        foreach (var subject in subjects)
        {
            var position = GetPosition(userId, subject.Value.queue, subject.Value.waiting);
            builder.AppendLine(string.Format(PositionPattern, subject.Key, position));
        }

        return builder.ToString();
    }

    public static string GetBySubject(long userId, string subjectName, List<long> queue, List<long> waiting)
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