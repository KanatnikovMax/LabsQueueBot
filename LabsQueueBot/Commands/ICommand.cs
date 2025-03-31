using LabsQueueBot.Db.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot.Commands;

public interface ICommand
{
    string Name { get; }
    User.UserState State { get; }
    UserRule.Rule AcceptUserUserRule { get; }
    string? Definition { get; }
    Task Execute(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}