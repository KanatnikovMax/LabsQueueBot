using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

/// <summary>
/// Вызывает показ очереди по дисциплине
/// </summary>
public class Show : Command
{
    public override string Definition => "/show";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        long id = update.Message.Chat.Id;
        User user = Users.At(update.Message.Chat.Id);
        List<string> subjects = Groups.At(new GroupKey(user.CourseNumber, user.GroupNumber)).Keys.ToList();
        bool addFlag = Users.At(id).State == User.UserState.Join;
        return KeyboardCreator.ListToKeyboard(subjects, addFlag, true, 1);
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        Users.At(id).State = User.UserState.ShowQueue;
        return new SendMessageRequest(id, "Выберите предмет:");
    }
}