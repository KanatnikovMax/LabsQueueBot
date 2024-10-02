using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

//создание пользователя(id, name) в словаре
public class StartApplier : Command
{
    public override string Definition => "/start_applier";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return new SetGroup().GetKeyboard(update);
    }

    public override SendMessageRequest Run(Update update)
    {

        long id = update.Message.Chat.Id;

        var data = update.Message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        Users.At(id).State = User.UserState.None;
        if (data.Length != 2)
        {
            Users.Remove(id);
            return new SendMessageRequest(id, "Твое имя - ошибка, и жизнь твоя - ошибка");
        }
        try
        {
            Users.Add(id, $"{data[0]} {data[1]}");
            Users.At(id).State = User.UserState.UnsetStudentData;
        }
        catch (ArgumentException exception)
        {
            Users.Remove(id);
            return new SendMessageRequest(update.Message.Chat.Id, exception.Message);
        }
        return new SendMessageRequest(id, "Выберите свои курс и группу:");
    }
}