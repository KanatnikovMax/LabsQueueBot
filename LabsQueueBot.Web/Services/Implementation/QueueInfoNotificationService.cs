using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.Repository.Repository;
using Telegram.Bot;

namespace LabsQueueBot.Web.Services.Implementation;

public class QueueInfoNotificationService(
    IServiceScopeFactory scopeFactory,
    ITelegramBotClient botClient) : IQueueInfoNotificationService
{
    public async Task NotifyAll(CancellationToken cancellationToken)
    {
        // TODO подумать про закрепление последнего расписания (расписания или очереди? если очереди - нафиг)
        await using var scope = scopeFactory.CreateAsyncScope();
        
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var subjectRepository = scope.ServiceProvider.GetRequiredService<ISubjectRepository>();
        
        var subjects = (await subjectRepository.GetAllAsync(cancellationToken))
            .ToLookup(s => (s.CourseNumber, s.GroupNumber));
        var users = (await userRepository.GetAllAsync(cancellationToken))
            .Where(u => u.State == UserState.None)
            .ToLookup(u => (u.CourseNumber, u.GroupNumber));
            
        var sendList = new List<Task>();
        
        foreach (var usersByGroup in users)
        {
            if (!subjects.Contains(usersByGroup.Key))
                continue;

            var subjectsByGroup = subjects[usersByGroup.Key]
                .ToDictionary(s => s.SubjectName, s => (s.Queue.ToList(), s.Waiting.ToList()));
            
            foreach (var user in usersByGroup)
            {
                var message = QueueInfoBuildHelper.GetByUser(user.Id, subjectsByGroup);
                
                var task = botClient.SendTextMessageAsync(
                    chatId: user.Id,
                    text: message,
                    cancellationToken: cancellationToken);
                sendList.Add(task);
            }
        }
        
        Task.WaitAll(sendList.ToArray(), cancellationToken);
    }

    public async Task NotifyGroup(byte course, byte group, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var subjectRepository = scope.ServiceProvider.GetRequiredService<ISubjectRepository>();
        
        //// TODO проверить, работает ли без UserState.None
        // var users = (await userRepository.GetByConditionAsync(
        //         u => u.CourseNumber == course && u.GroupNumber == group && u.State == UserState.None,
        //         cancellationToken))
        //     .ToList();
        var users = (await userRepository.GetGroup(course, group, cancellationToken))
            .ToList();
        var subjects = (await subjectRepository.GetByConditionAsync(
                s => s.CourseNumber == course && s.GroupNumber == group,
                cancellationToken))
            .ToDictionary(s => s.SubjectName, s => (s.Queue.ToList(), s.Waiting.ToList()));

        var tasks = users
            .Select(Task (user) => 
                Task.Run(() => 
                {
                    var message = QueueInfoBuildHelper.GetByUser(user.Id, subjects);
                    botClient.SendTextMessageAsync(
                        chatId: user.Id,
                        text: message,
                        cancellationToken: cancellationToken);
                },
                cancellationToken))
            .ToArray();
        Task.WaitAll(tasks, cancellationToken);
    }

    public async Task NotifyGroupBySubject(byte course, byte group, string subjectName, List<long> queue, List<long> waiting, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
        //// TODO проверить, работает ли без UserState.None
        // var users = (await userRepository.GetByConditionAsync(
        //         u => u.CourseNumber == course && u.GroupNumber == group && u.State == UserState.None,
        //         cancellationToken))
        //     .ToList();
        var users = (await userRepository.GetGroup(course, group, cancellationToken))
            .ToList();

        var tasks = users
            .Select(Task (user) =>
            Task.Run(() => 
                {
                    var message = QueueInfoBuildHelper.GetSingleBySubject(user.Id, subjectName, queue, waiting);
                    botClient.SendTextMessageAsync(
                        chatId: user.Id,
                        text: message,
                        cancellationToken: cancellationToken);
                },
                cancellationToken))
            .ToArray();
        Task.WaitAll(tasks, cancellationToken);
    }

    public async Task NotifyUserBySubject(long userId, string subjectName, CancellationToken cancellationToken, byte? course = null, byte? group = null)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
        if (course == null || group == null)
        {
            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return;

            course = user.CourseNumber;
            group = user.GroupNumber;
        }
        
        var subjectRepository = scope.ServiceProvider.GetRequiredService<ISubjectRepository>();

        var subject = await subjectRepository.GetByGroupAndName(course.Value, group.Value, subjectName, cancellationToken);
        if (subject == null)
            return;

        var queue = (await userRepository.GetByConditionAsync(
                u => subject.Queue.Contains(u.Id),
                cancellationToken))
            .Join(subject.Queue, u => u.Id, id => id,
                (u, id) => (u.Name, subject.Queue.ToList().IndexOf(id)))
            .OrderBy(x => x.Item2)
            .ToList();
        
        var waiting = (await userRepository.GetByConditionAsync(
                u => subject.Waiting.Contains(u.Id),
                cancellationToken))
            .Select(u => u.Name)
            .ToList();
        
        var message = QueueInfoBuildHelper.GetAllBySubject(subject.SubjectName, queue, waiting);
        await botClient.SendTextMessageAsync(
            chatId: userId,
            text: message,
            cancellationToken: cancellationToken);
    }

    public async Task NotifyGroupAboutTimetable(byte course, byte group, string timetableMessage, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
        //// TODO проверить, работает ли без UserState.None
        // var users = (await userRepository.GetByConditionAsync(
        //         u => u.CourseNumber == course && u.GroupNumber == group && u.State == UserState.None,
        //         cancellationToken))
        //     .ToList();
        var users = (await userRepository.GetGroup(course, group, cancellationToken))
            .ToList();

        var tasks = users
            .Select(Task (user) =>
                Task.Run(() => 
                    {
                        botClient.SendTextMessageAsync(
                            chatId: user.Id,
                            text: timetableMessage,
                            cancellationToken: cancellationToken);
                    },
                    cancellationToken))
            .ToArray();
        Task.WaitAll(tasks, cancellationToken);
    }
}