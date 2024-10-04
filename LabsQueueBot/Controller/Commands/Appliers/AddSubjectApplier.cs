using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

/// <summary>
/// Добавляет новую дисциплину в группу
/// </summary>
public class AddSubjectApplier : Command
{
    public override string Definition => "/add_subject_applier";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        string subject = update.Message.Text.Trim();
        User user = Users.At(id);
        Group group = Groups.At(new GroupKey(user.CourseNumber, user.GroupNumber));
        user.State = User.UserState.None;
        try
        {
            group.AddSubject(subject);
            return new SendMessageRequest(id,
                $"Очередь по предмету {subject} добавлена\nНажмите /join для добавления в очередь");
        }
        catch (Exception exception)
        {
            return new SendMessageRequest(id, exception.Message);
        }
    }
}