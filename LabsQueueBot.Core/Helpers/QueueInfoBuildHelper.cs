using System.Text;

namespace LabsQueueBot.Core.Helpers;

public static class QueueInfoBuildHelper
{
    private const string OutOfSubject = "отсутствует";
    private const string Waiting = "в ожидании";
    
    public static string GetByUser(long userId, Dictionary<string, (List<long> queue, List<long> waiting)> subjects)
    {
        var builder = new StringBuilder();
        
        builder.AppendLine("Очереди твоего курса и твои номера в них:");
        foreach (var subject in subjects)
        {
            var position = GetPosition(userId, subject.Value.queue, subject.Value.waiting);
            builder.AppendLine($"{subject.Key} -> {position}");
        }

        return builder.ToString();
    }

    public static string GetBySubject(long userId, string subject, List<long> queue, List<long> waiting)
    {
        var builder = new StringBuilder();
        
        builder.AppendLine("Очереди твоего курса и твои номера в них:");
        
        // var position = GetPosition(userId, waiting);
        // builder.AppendLine($"{subject.Key} -> {position}");

        return builder.ToString();
    }

    private static string GetPosition(long userId, List<long> queue, List<long> waiting)
    {
        string position;
        if (queue.Contains(userId))
        {
            position = (1 + queue.IndexOf(userId)).ToString();
        }
        else if (waiting.Contains(userId))
        {
            position = Waiting;
        }
        else
        {
            position = OutOfSubject;
        }

        return position;
    }
}