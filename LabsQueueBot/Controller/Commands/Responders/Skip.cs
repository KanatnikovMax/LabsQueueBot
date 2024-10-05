using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

//пропустить человека вперед себя
public class Skip : Command
{
    public override string Definition => "/skip - Пропустить одного человека вперёд себя";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return new Show().GetKeyboard(update);
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        Users.At(id).State = User.UserState.Skip;
        return new SendMessageRequest(id, "Выберите предмет:");
    }
}