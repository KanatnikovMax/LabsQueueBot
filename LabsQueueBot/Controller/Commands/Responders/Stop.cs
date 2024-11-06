using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

/// <summary>
/// Отписывает пользователя от бота
/// </summary>
public class Stop : Command
{
    public override string Definition => "/stop - Отписаться";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        Groups.Remove(id);
        Users.Remove(id);
        return new SendMessageRequest(id, "Прощай, мой друг");
    }
}