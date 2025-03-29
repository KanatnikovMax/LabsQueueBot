using LabsQueueBot.Model;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot.Controller.Commands.Responders;

public class SetTimetable : Command
{
    public override string Definition => "/set_timetable";
    
    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return new Show().GetKeyboard(update);
    }
    
    public override SendMessageRequest Run(Update update)
    {
        var id = update.Message.Chat.Id;
        Users.At(id).State = User.UserState.SetTimetable;
        return new SendMessageRequest(id, "Выберите предмет, для которого хотите задать расписание формирования очередей");
    }
}