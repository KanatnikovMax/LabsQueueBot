using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

public class ShowQueueApplier : Command
{
    public override string Definition => "/show_queue_applier";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        var subject = update.CallbackQuery.Data;
        var id = update.CallbackQuery.Message.Chat.Id;
        if (update.CallbackQuery.Message.Text != "Выберите предмет:")
            throw new InvalidOperationException();

        if (subject == "Назад")
            return new SendMessageRequest(id, subject);

        User user = Users.At(id);
        if (subject == "Добавить")
        {
            user.State = User.UserState.AddSubject;
            return new SendMessageRequest(id, "Введите название нового предмета:");
        }

        Group group = Groups.At(new GroupKey(user.CourseNumber, user.GroupNumber));
            
        return new SendMessageRequest(id, Groups.ShowQueue(id, subject));
    }
}