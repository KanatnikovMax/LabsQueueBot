using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

/// <summary>
/// Вызывает меню с дисциплинами для показа очередей по ним
/// </summary>
public class ShowQueue : Command
{
    public override string Definition => "/show - Показать очередь полностью";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return new Show().GetKeyboard(update);
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        Users.At(id).State = User.UserState.ShowQueue;
        return new SendMessageRequest(id, "Выберите предмет:");
    }
}