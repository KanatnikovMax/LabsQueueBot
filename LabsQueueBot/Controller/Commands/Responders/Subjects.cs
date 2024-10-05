using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

//показывает предметы и номера в очереди по ним
public class Subjects : Command
{
    public override string Definition => "/subjects - Список предметов и номера в очередях по ним";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {

        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        return new SendMessageRequest(id, Groups.ShowSubjects(id));
    }
}