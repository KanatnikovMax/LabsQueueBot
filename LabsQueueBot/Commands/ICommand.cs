using Telegram.Bot;
using Telegram.Bot.Types;

namespace LabsQueueBot.Commands;

public interface ICommand
{
    string Name { get; }
    string? Definition { get; }
    Task Execute(ITelegramBotClient botClient, Update update, CancellationToken stoppingToken);
}