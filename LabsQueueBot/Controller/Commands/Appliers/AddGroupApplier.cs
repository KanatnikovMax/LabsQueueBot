using System.Text;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

public class AddGroupApplier : Command
{
    public override string Definition => "/add_group_applier";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        string[] text = update.Message.Text.Split(':');

        byte course;
        byte group;

        if (text.Length != 2 || !Byte.TryParse(text[0], out course) || !Byte.TryParse(text[1], out group))
            return new SendMessageRequest(id, "Некорректные данные\nПовторите ввод");

        if (course == Users.At(id).CourseNumber && group == Users.At(id).GroupNumber)
            return new SendMessageRequest(id, "Ты итак в этой группе, дурачок\nПовтори ввод");

        StringBuilder builder = new StringBuilder();
            
        try
        {
            Groups.Remove(id);
            Groups.Add(course, group);
            var newUser = new User(course, group, Users.At(id).Name, id)
            {
                State = User.UserState.None
            };
            Users.Add(newUser);
            Groups.At(new GroupKey(course, group)).AddStudent();
            var str = new Help().Run(update).Text;
            builder.AppendLine($"Вы были добавлены в {course} курс {group} группу\n{str}");
        }
        catch (ArgumentException exception)
        {
            builder.AppendLine(exception.Message);
            builder.AppendLine("Повторите ввод:");
        }
        catch (InvalidDataException exception)
        {
            var newUser = new User(course, group, Users.At(id).Name, id)
            {
                State = User.UserState.None
            };
            Users.Add(newUser);
            Groups.At(new GroupKey(course, group)).AddStudent();

            builder.AppendLine(exception.Message);
            var str = new Help().Run(update).Text;
            builder.AppendLine($"Вы были добавлены в {course} курс {group} группу\n{str}");
        }
        catch (InvalidOperationException exception)
        {
            builder.AppendLine(exception.Message);
            builder.AppendLine("Повторите ввод:");
        }

        return new SendMessageRequest(id, builder.ToString());
    }
}