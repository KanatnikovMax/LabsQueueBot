using LabsQueueBot.Commands;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot.Bot;

public class UpdateHandler : IUpdateHandler
{
    private readonly CommandFactory _commandFactory;
    private readonly IRepository<User> _usersRepository;

    public UpdateHandler(CommandFactory commandFactory, IRepository<User> usersRepository)
    {
        _commandFactory = commandFactory;
        _usersRepository = usersRepository;
    }
    
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken stoppingToken)
    {
        var user = await _usersRepository.GetByIdAsync(update.Message.Chat.Id);
        if (user is null)
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Вы не зарегистрированы!\n/start для регистрации",
                cancellationToken: stoppingToken);
        }
        
        switch (update.Type)
        {
            case UpdateType.Message:
                await HandleMessage(botClient, update, stoppingToken);
                break;
            
            case UpdateType.CallbackQuery:
                await HandleCallbackQuery(botClient, update, stoppingToken);
                break;
            
            case UpdateType.MyChatMember:
                await HandleMyChatMember(botClient, update, stoppingToken);
                break;
                
            default:
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "Введи команду, ящур",
                    cancellationToken: stoppingToken);
                break;
        }
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken stoppingToken)
    {
        Console.WriteLine(JsonConvert.SerializeObject(exception));
        var lastUpdates = await botClient.GetUpdatesAsync(
            offset: 10,
            limit: 10,
            cancellationToken: stoppingToken);
        foreach (var update in lastUpdates
                     .Where(update =>
                         update.Type == UpdateType.CallbackQuery))
        {
            Console.WriteLine(JsonConvert.SerializeObject(update));
        }
    }
    
    private async Task HandleMessage(ITelegramBotClient botClient, Update update, CancellationToken stoppingToken)
    {
        var id = update.Message.Chat.Id;
        var user = await _usersRepository.GetByIdAsync(id);
        
        // Если есть активная команда - передаем управление ей
        if (state.CurrentCommand != null)
        {
            var command = _commandFactory.GetCommand(state.CurrentCommand);
            await command.Execute(botClient, update, stoppingToken);
        }
        else
        {
            // Обработка новых команд
            var command = _commandFactory.GetCommand(update.Message.Text);
            await command?.Execute(botClient, update, stoppingToken);
        }
    }
    
    private async Task HandleCallbackQuery(ITelegramBotClient botClient, Update update, CancellationToken stoppingToken)
    {
        var callbackQuery = update.CallbackQuery;
        var state = _stateRepo.GetState(callbackQuery.Message.Chat.Id);
        
        if (state.CurrentCommand == nameof(ShowItemsCommand) && callbackQuery.Data.StartsWith("day_"))
        {
            var selectedDay = callbackQuery.Data.Split('_')[1];
            var items = _shoppingService.GetItemsForDay(selectedDay);
            
            await botClient.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                $"Список покупок на {selectedDay}:\n{string.Join("\n", items)}",
                cancellationToken: stoppingToken);
            
            _stateRepo.ResetState(callbackQuery.Message.Chat.Id);
        }
    }

    private async Task HandleMyChatMember(ITelegramBotClient botClient, Update update, CancellationToken stoppingToken)
    {
        var id = update.MyChatMember.Chat.Id;
        var user = await _usersRepository.GetByIdAsync(id);
        await _usersRepository.DeleteAsync(user);
    }
}