using LabsQueueBot.Core.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands;

public interface ICommandExecutor
{
    string? Type { get; }
    string Name { get; }
    IReadOnlyCollection<(UserState State, UpdateType Type)> Allows { get; } 
    Role AcceptRole { get; }
    string? Definition { get; }
    Task Execute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken);
}