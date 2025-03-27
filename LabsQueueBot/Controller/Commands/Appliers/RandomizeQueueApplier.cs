using LabsQueueBot.Model;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot.Controller.Commands.Appliers;

public class RandomizeQueueApplier : Command
{
    public override string Definition => "/randomize_queue_applier";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        var subject = update.CallbackQuery.Data;
        var id = update.CallbackQuery.Message.Chat.Id;

        if (update.CallbackQuery.Message.Text != "Выберите предмет, очередь по которому необходимо сформировать:")
            throw new InvalidOperationException();

        // отмена объединения
        if (subject == "Назад")
            return new SendMessageRequest(id, "Предмет не выбран, очередь не будет объединена");

        // объединение очереди по выбранной дисциплине
        var user = Users.At(id);
        Groups.At(new GroupKey(user.CourseNumber, user.GroupNumber))[subject].Union();

        return new SendMessageRequest(id, "Очередь по выбранному предмету сформирована");
    }
}