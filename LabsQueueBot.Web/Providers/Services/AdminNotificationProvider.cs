using LabsQueueBot.Core.Constants;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Repository.Repository;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LabsQueueBot.Web.Providers.Services;

public class AdminNotificationProvider(
    IServiceScopeFactory scopeFactory,
    ITelegramBotClient botClient) : IAdminNotificationProvider
{
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