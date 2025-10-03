using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Repository.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace LabsQueueBot.BusinessLogic.Services.Implementation;

public class UserStateCleanerService(
    ITelegramBotClient botClient,
    IServiceScopeFactory scopeFactory,
    IOptions<TelegramBotSettings> options) : IUserStateCleanerService
{
    public async Task ClearAll(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var users = (await GetUsersToClear(userRepository, cancellationToken))
            .ToList();

        if (users.Count != 0)
        {
            var markupsToClear = users
                .Where(x => x.LastCallbackableMessageId != null)
                .Select(x => new { x.Id, x.LastCallbackableMessageId })
                .ToDictionary(x => x.Id, x => x.LastCallbackableMessageId!.Value);
            
            var messagesToClear = users
                .Where(x => x.LastCallbackableMessageId == null)
                .Select(x => x.Id)
                .ToList();
            
            foreach (var user in users)
            {
                user.State = UserState.None;
                user.LastCallbackableMessageId = null;
            }
            await userRepository.UpdateBatchAsync(users, cancellationToken);

            var clearMarkups = ClearReplyMarkupsInChats(markupsToClear, cancellationToken);
            var clearMessages = ClearReplyMessagesInChats(messagesToClear, cancellationToken);
            Task.WaitAll([clearMarkups, clearMessages], cancellationToken);
        }
    }

    public async Task ClearOrDeleteAll(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var deleteUnregisteredUsersTask = userRepository.DeleteByConditionAsync(x => 
                x.State == UserState.Unregistered 
                && x.LastActivityAt.AddMinutes(options.Value.StateAllowedIntervalInMinutes) < DateTime.UtcNow, 
            cancellationToken);

        var users = (await GetUsersToClear(userRepository, cancellationToken))
            .ToList();

        if (users.Count != 0)
        {
            var markupsToClear = users
                .Where(x => x.LastCallbackableMessageId != null)
                .Select(x => new { x.Id, x.LastCallbackableMessageId })
                .ToDictionary(x => x.Id, x => x.LastCallbackableMessageId!.Value);

            var messagesToClear = users
                .Where(x => x.LastCallbackableMessageId == null)
                .Select(x => x.Id)
                .ToList();
            
            foreach (var user in users)
            {
                user.State = UserState.None;
                user.LastCallbackableMessageId = null;
            }
            await userRepository.UpdateBatchAsync(users, cancellationToken);
            
            var clearMarkups = ClearReplyMarkupsInChats(markupsToClear, cancellationToken);
            var clearMessages = ClearReplyMessagesInChats(messagesToClear, cancellationToken);
            Task.WaitAll([clearMarkups, clearMessages], cancellationToken);
        }

        await deleteUnregisteredUsersTask;
    }

    private async Task<IEnumerable<User>> GetUsersToClear(IUserRepository userRepository, CancellationToken cancellationToken)
    {
        return await userRepository.GetByConditionAsync(u =>
                u.State != UserState.None
                && u.State != UserState.Register        // ответ на текстовое сообщение регистрации
                && u.State != UserState.Unregistered    // удаляются, а не очищаются
                && u.LastActivityAt.AddMinutes(options.Value.StateUpdateTimeoutInMinutes) < DateTime.UtcNow,
            cancellationToken);
    }

    private Task ClearReplyMarkupsInChats(IReadOnlyDictionary<long, int> markupsToClear, CancellationToken cancellationToken)
    {
        var clearTasks = markupsToClear
            .Select(Task (markup) =>
                Task.Run(() => 
                        botClient.EditMessageReplyMarkupAsync(
                            chatId: markup.Key, 
                            messageId: markup.Value, 
                            replyMarkup: null, 
                            cancellationToken: cancellationToken), 
                    cancellationToken)).ToArray();
        
        Task.WaitAll(clearTasks, cancellationToken);
        return Task.CompletedTask;
    }

    private Task ClearReplyMessagesInChats(IReadOnlyCollection<long> messagesToClear, CancellationToken cancellationToken)
    {
        var clearTasks = messagesToClear
            .Select(Task (chatId) =>
                Task.Run(() => 
                        botClient.SendTextMessageAsync(
                            chatId: chatId, 
                            text: "Время на ответ вышло",
                            cancellationToken: cancellationToken), 
                    cancellationToken)).ToArray();
        
        Task.WaitAll(clearTasks, cancellationToken);
        return Task.CompletedTask;
    }
}