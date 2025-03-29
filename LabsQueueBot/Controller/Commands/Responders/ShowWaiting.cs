using LabsQueueBot.Model;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot.Controller.Commands.Responders;

public class ShowWaiting : Command
{
    public override string Definition => "/show_waiting - Показать список ожидания для выбранной очереди";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return new Show().GetKeyboard(update);
    }

    public override SendMessageRequest Run(Update update)
    {
        var id = update.Message.Chat.Id;
        Users.At(id).State = User.UserState.ShowWaiting;
        return new SendMessageRequest(id, "Выберите предмет:");
    }
}