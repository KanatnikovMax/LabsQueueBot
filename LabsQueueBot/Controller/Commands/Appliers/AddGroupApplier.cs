using System.Text;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

/// <summary>
/// Добавляет новую группу и записывает туда пользователя; если группа уже существует - записывает пользователя в нее
/// </summary>
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

        //невалидные данные
        if (text.Length != 2 || !Byte.TryParse(text[0], out course) || !Byte.TryParse(text[1], out group))
            return new SendMessageRequest(id, "Некорректные данные\nПовторите ввод");

        if (course == Users.At(id).CourseNumber && group == Users.At(id).GroupNumber)
            return new SendMessageRequest(id, "Ты уже находишься в этой группе\nПовтори ввод");

        StringBuilder builder = new StringBuilder();

        //если группа не существует
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
        //невалидные курс и группа
        catch (ArgumentException exception)
        {
            builder.AppendLine(exception.Message);
            builder.AppendLine("Повторите ввод:");
        }
        //такая группа уже существует
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
        //невозможно добавить пользователя в группу
        catch (InvalidOperationException exception)
        {
            builder.AppendLine(exception.Message);
            builder.AppendLine("Повторите ввод:");
        }

        return new SendMessageRequest(id, builder.ToString());
    }
}