using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

public class SkipApplier : Command
{
    public override string Definition => "/skip_applier";

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
        try
        {
            if (!group.ContainsKey(subject))
                return new SendMessageRequest(id, "Тебя тут нет, кого ты пропускаешь ?");
            group[subject].Skip(id);
            return new SendMessageRequest(id, "Это как шаг вперед, но назад");
        }
        catch (InvalidOperationException exception)
        {
            return new SendMessageRequest(id, exception.Message);
        }
    }
}