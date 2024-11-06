using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

/// <summary>
/// Отправляет клавиатуру для выбора дисциплины, в очередь по которым можно встать
/// </summary>
public class Join : Command
{
    public override string Definition => "/join - Встать в очередь";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return new Show().GetKeyboard(update);
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        Users.At(id).State = User.UserState.Join;
        return new SendMessageRequest(id, "Выберите предмет:");
    }
}