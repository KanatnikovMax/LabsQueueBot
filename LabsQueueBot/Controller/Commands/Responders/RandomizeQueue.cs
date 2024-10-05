using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

// админская команда рандома очереди
public class RandomizeQueue : Command
{
    public override string Definition => "/randomize_queue - Зарандомить очередь";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        var user = Users.At(id);
        Groups.At(new GroupKey(user.CourseNumber, user.GroupNumber)).Union();
        return new SendMessageRequest(id, "Очереди в группе сформированы");
    }
}