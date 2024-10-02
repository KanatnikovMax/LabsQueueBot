using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

public class JoinApplier : Command
{
    public override string Definition => "/join_applier";

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

        if (subject == "Добавить")
        {
            user.State = User.UserState.AddSubject;
            return new SendMessageRequest(id, "Введите название нового предмета:");
        }
        if (!group.ContainsKey(subject))
            group.AddSubject(subject);
        int position = group.AddStudent(id, subject);
        if (position == -1)
            return new SendMessageRequest(id, $"Ты добавлен в список ожидания");
        if (position == -2)
            return new SendMessageRequest(id, "Ты находишься в списке ожидания");
        return new SendMessageRequest(id, $"Ты уже записан в эту очередь\nТвой номер в очереди — {group[subject].Position(id) + 1}");
    }
}