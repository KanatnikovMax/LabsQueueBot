using LabsQueueBot.Model;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot.Controller.Commands.Responders;

/// <summary>
/// Отвечает на админскую команду: объединения очередей и списков ожидания
/// </summary>
public class RandomizeQueue : Command
{
    public override string Definition => "/randomize_queue - Зарандомить очередь";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return new Show().GetKeyboard(update);
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        Users.At(id).State = User.UserState.Union;
        return new SendMessageRequest(id, "Выберите предмет, очередь по которому необходимо сформировать:");
    }
}