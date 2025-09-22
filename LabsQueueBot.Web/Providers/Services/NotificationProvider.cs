using LabsQueueBot.Core.Constants;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.Repository.Repository;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LabsQueueBot.Web.Providers.Services;

public class NotificationProvider(
    ITelegramBotClient botClient,
    IServiceScopeFactory scopeFactory) : INotificationProvider
{
    public async Task NotifyAll(CancellationToken cancellationToken)
    {
        // TODO подумать про закрепление последнего расписания
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
                    var message = QueueInfoBuildHelper.GetBySubject(user.Id, subjectName, queue, waiting);
                    botClient.SendTextMessageAsync(
                        chatId: user.Id,
                        text: message,
                        cancellationToken: cancellationToken);
                },
                cancellationToken))
            .ToArray();
        Task.WaitAll(tasks, cancellationToken);
    }

    public async Task NotifyUserBySubject(long userId, string subjectName, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return;
        
        var subjectRepository = scope.ServiceProvider.GetRequiredService<ISubjectRepository>();

        var subject = await subjectRepository.GetByGroupAndName(user.CourseNumber, user.GroupNumber, subjectName, cancellationToken);
        if (subject == null)
            return;
        
        var message = QueueInfoBuildHelper.GetBySubject(user.Id, subject.SubjectName, subject.Queue.ToList(), subject.Waiting.ToList());
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: message,
            cancellationToken: cancellationToken);
    }
    
    public async Task NotifyAdminsWithDocument(int documentId, string message, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
        var users = (await userRepository.GetByConditionAsync(
                u => u.Role == Role.Admin, // && u.State == UserState.None,
                cancellationToken))
            .ToList();

        var path = string.Format(GlobalConstants.ErrorDocumentPath, documentId);
        
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var document = new InputFileStream(stream, path);
        
        var tasks = users
            .Select(Task (user) =>
                Task.Run(() => 
                    {
                        botClient.SendDocumentAsync(
                            chatId: user.Id,
                            document: document,
                            caption: message,
                            cancellationToken: cancellationToken);
                    },
                    cancellationToken))
            .ToArray();
        Task.WaitAll(tasks, cancellationToken);
    }
}