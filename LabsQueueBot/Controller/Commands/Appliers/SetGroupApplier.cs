using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

/// <summary>
/// Измененяет курс и группу пользователя
/// </summary>
public class SetGroupApplier : Command
{
    public override string Definition => "/set_group_applier";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        var text = update.CallbackQuery.Data;
        var id = update.CallbackQuery.Message.Chat.Id;

        if (update.CallbackQuery.Message.Text != "Выберите свои курс и группу:")
            throw new InvalidOperationException();

        //отмена изменения
        if (text == "Назад")
            return new SendMessageRequest(id, text);

        //если были выбраны существующие курс и группа
        if (text != "Добавить")
        {
            var line = text.Split(' ');
            byte course = Convert.ToByte(line[0]);
            byte group = Convert.ToByte(line[2]);

            if (Users.At(id).State == User.UserState.ChangeData)
                Groups.Remove(id);

            try
            {
                //если группы не существует
                if (!Groups.ContainsKey(new GroupKey(course, group)))
                    Groups.Add(course, group);

                if (!Groups.At(new GroupKey(course, group)).AddStudent())
                {
                    throw new InvalidOperationException("Много студентов в группе");
                }

                var newUser = new User(course, group, Users.At(id).Name, id)
                {
                    State = User.UserState.None
                };
                Users.Add(newUser);
                return new Help().Run(update);
            }
            //невалидные курс и группа
            catch (ArgumentException exception)
            {
                return new SendMessageRequest(id, $"{exception.Message}\nПовторите ввод:");
            }
            //если курс и группа уже существуют
            catch (InvalidDataException exception)
            {
                return new SendMessageRequest(id, exception.Message);
            }
            //превышено количество студентов в группе
            catch (InvalidOperationException exception)
            {
                return new SendMessageRequest(id, exception.Message);
            }
        }
        else
            Users.At(id).State = User.UserState.AddGroup;

        return new SendMessageRequest(id, "Введите свои курс и группу. Например, для n курса m группы\nn:m");
    }
}