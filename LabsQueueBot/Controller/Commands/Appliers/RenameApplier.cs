using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

public class RenameApplier : Command
{
    public override string Definition => "/rename_applier";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        var data = update.Message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        Users.At(id).State = User.UserState.None;
        if (data.Length != 2)
        {
            return new SendMessageRequest(id, "Смена личности не удалась");
        }
        try
        {
            var name = $"{data[0]} {data[1]}";
            Users.At(id).Name = name;
            using (var db = new QueueBotContext())
            {
                var user = db.UserRepository.FirstOrDefault(u =>  u.Id == id);
                user.Name = name;
                db.UserRepository.Update(user);
                db.SaveChanges();
            }
        }
        catch (ArgumentException exception)
        {
            return new SendMessageRequest(update.Message.Chat.Id, exception.Message);
        }
        return new SendMessageRequest(id, "Смена личности завершена");
    }


}