using LabsQueueBot.Core.Constants;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Repository.Repository;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

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
        var fileBytes = await File.ReadAllBytesAsync(path, cancellationToken);
        var fileName = Path.GetFileName(path);
        
        var tasks = users
            .Select(user =>
            {
                var fileStream = new MemoryStream(fileBytes, writable: false);
                return botClient.SendDocumentAsync(
                    chatId: user.Id,
                    document: InputFile.FromStream(fileStream, fileName),
                    caption: message,
                    cancellationToken: cancellationToken);
            });
        await Task.WhenAll(tasks);
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
            .Select(user =>
                Task.Run(() => 
                    botClient.SendTextMessageAsync(
                        chatId: user.Id,
                        text: message,
                        cancellationToken: cancellationToken),
                    cancellationToken));
        await Task.WhenAll(tasks);
    }
}