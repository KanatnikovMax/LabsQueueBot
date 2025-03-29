using LabsQueueBot.Model;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot.Controller.Commands.Appliers;

public class SetTimetableApplier : Command
{
    public override string Definition => "/set_timetable_applier";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        var id = update.CallbackQuery.Message.Chat.Id;
        var subject = update.CallbackQuery.Data;
        
        if (update.CallbackQuery.Message.Text != "Выберите предмет, для которого хотите задать расписание формирования очередей")
            throw new InvalidOperationException();
        
        if (subject == "Назад")
            return new SendMessageRequest(id, subject);

        var user = Users.At(id);
        user.State = User.UserState.SetTimetableDays;
        Groups.groups[new GroupKey(user.CourseNumber, user.GroupNumber)].SubjectsToSetTimetable.Add(id, subject);
        
        return new SendMessageRequest(id, "Введите дни недели, разделяя их пробелом");
    }
}