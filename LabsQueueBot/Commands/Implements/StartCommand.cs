using LabsQueueBot.Bot;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot.Commands.Implements;

public class StartCommand : ICommand
{
    private IRepository<User> _usersRepository;
        
    public string Name => "/show";
    
    public string Definition => "";

    public StartCommand(ILogger _logger, IRepository<User> usersRepository)
    {
        _usersRepository = usersRepository;
    }
    
    public async Task Execute(ITelegramBotClient botClient, Update update, CancellationToken stoppingToken)
    {
        var id = update.Message.Chat.Id;

        var user = await _usersRepository.GetByIdAsync(id);
        if (user is not null)
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Ты уже зареган\nИди отсюда, розбийник",
                cancellationToken: stoppingToken);
        }
        
        user = new User(id)
        {
            State = User.UserState.Unregistred
        };
        await _usersRepository.SaveAsync(user);
        
        await botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Кто ты, воин?\n\nВведи свои данные в формате\nФамилия Имя",
            cancellationToken: stoppingToken);
    }
}