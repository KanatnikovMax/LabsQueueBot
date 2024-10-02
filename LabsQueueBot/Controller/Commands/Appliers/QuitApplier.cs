using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

public class QuitApplier : Command
{
    public override string Definition => "/quit_applier";

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
        Group group = Groups.At(new GroupKey(user.CourseNumber, user.GroupNumber));

        if (group.ContainsKey(subject) && group.RemoveStudentFromQueue(id, subject))
            return new SendMessageRequest(id, "Ты вышел из очереди");

        return new SendMessageRequest(id, "Ты не можешь выйти из очереди, в которой не числишься");
    }
}