using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;
/// <summary>
/// Изменяет личные данные пользователя (фамилию, имя);
/// обновляет БД
/// </summary>
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
        //проверка валидности
        if (data.Length != 2 && data.Any(c => "0123456789~!@#$%^&*()_+{}:\"|?><`=[]\\;',./№".Contains(c)))
        {
            return new SendMessageRequest(id, "Смена личности не удалась");
        }
        try
        {
            //обновление данных
            var name = $"{data[0]} {data[1]}";          
            using (var db = new QueueBotContext())
            {
                var user = db.UserRepository.FirstOrDefault(u =>  u.Id == id);
                user.Name = name;
                db.UserRepository.Update(user);
                db.SaveChanges();
            }
            Users.At(id).Name = name;
        }
        catch (ArgumentException exception)
        {
            return new SendMessageRequest(update.Message.Chat.Id, exception.Message);
        }
        return new SendMessageRequest(id, "Смена личности завершена");
    }


}