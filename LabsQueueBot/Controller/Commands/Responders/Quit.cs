using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

/// <summary>
/// Отправляет клавиатуру для выбора дисциплины, из очереди по которой можно выйти
/// </summary>
public class Quit : Command
{
    public override string Definition => "/quit - Выйти из очереди";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return new Show().GetKeyboard(update);
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        Users.At(id).State = User.UserState.Quit;
        return new SendMessageRequest(id, "Выберите предмет:");
    }
}