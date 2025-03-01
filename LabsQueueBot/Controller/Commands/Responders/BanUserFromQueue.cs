using LabsQueueBot.Model;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot.Controller.Commands.Responders;

public class BanUserFromQueue : Command
{
    public override string Definition =>  "/ban_person - удалить пользователя из очереди";
    
    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return new Show().GetKeyboard(update);
    }
    
    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        var user = Users.At(id);

        var updateText = update.Message.Text.Split(' ');
        var userToRemoveName = updateText[2] + updateText[3];

        var userToRemoveId = Users.FindUser(user.CourseNumber, user.GroupNumber, userToRemoveName);
        if (userToRemoveId == -1)
        {
            return new SendMessageRequest(id, "Пользователя с таким именем в вашей группе не существует");
        }

        Users.At(id).State = User.UserState.Ban;
        Groups.AddToBan(id, userToRemoveId);
        return new SendMessageRequest(id, "Выберите очередь по предмету, из которой необходимо забанить пользователя");
    }
}