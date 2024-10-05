using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

// сменить имя пользователя
public class Rename : Command
{
    public override string Definition => "/rename - Смена фамилии и имени";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        Users.At(id).State = User.UserState.Rename;
        return new SendMessageRequest(id, "Кто таков будешь?\n(фамилия, имя)");
    }
}