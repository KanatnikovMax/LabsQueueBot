using LabsQueueBot.Core.Constants;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Repository.Repository;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LabsQueueBot.Web.Services.Implementation;

public class AdminNotificationService(
    IServiceScopeFactory scopeFactory,
    ITelegramBotClient botClient) : IAdminNotificationService
{
    public async Task NotifyWithDocument(int documentId, string message, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
        var users = (await userRepository.GetByConditionAsync(
                u => u.Role == Role.Admin,
                cancellationToken))
            .ToList();

        var path = string.Format(GlobalConstants.ErrorDocumentPath, documentId);
        
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var document = new InputFileStream(stream, path);
        
        var tasks = users
            .Select(Task (user) =>
                Task.Run(() => 
                        botClient.SendDocumentAsync(
                            chatId: user.Id,
                            document: document,
                            caption: message,
                            cancellationToken: cancellationToken),
                    cancellationToken))
            .ToArray();
        Task.WaitAll(tasks, cancellationToken);
    }

    public async Task NotifyWithMessage(string message, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
        var users = (await userRepository.GetByConditionAsync(
                u => u.Role == Role.Admin,
                cancellationToken))
            .ToList();
        
        var tasks = users
            .Select(Task (user) =>
                Task.Run(() => 
                    botClient.SendTextMessageAsync(
                        chatId: user.Id,
                        text: message,
                        cancellationToken: cancellationToken),
                    cancellationToken))
            .ToArray();
        Task.WaitAll(tasks, cancellationToken);
    }
}