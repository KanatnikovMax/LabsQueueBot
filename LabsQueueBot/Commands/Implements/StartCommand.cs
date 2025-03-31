using LabsQueueBot.Bot;
using LabsQueueBot.Db.Entities;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot.Commands.Implements;

public class StartCommand : ICommand
{
    private IRepository<User> _usersRepository;
        
    public string Name => "/show";

    public User.UserState State => User.UserState.None;
    public UserRule.Rule AcceptUserUserRule => UserRule.Rule.Use;

    public string Definition => "";

    public StartCommand(ILogger _logger, IRepository<User> usersRepository)
    {
        _usersRepository = usersRepository;
    }
    
    public async Task Execute(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var id = update.Message.Chat.Id;

        var user = await _usersRepository.GetByIdAsync(id, cancellationToken);
        if (user is not null)
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Ты уже зареган\nИди отсюда, розбийник",
                cancellationToken: cancellationToken);
        }
        
        user = new User(id)
        {
            State = User.UserState.Unregistred
        };
        await _usersRepository.SaveAsync(user, cancellationToken);
        
        await botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Кто ты, воин?\n\nВведи свои данные в формате\nФамилия Имя",
            cancellationToken: cancellationToken);
    }
}