using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

/// <summary>
/// Подписывает пользователя на бота
/// </summary>
public class Start : Command
{
    public override string Definition => "/start";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        if (Users.Contains(id))
        {
            return new SendMessageRequest(id, "Ты уже зареган\nИди отсюда, розбийник");
        }

        var newUser = new User(id)
        {
            State = User.UserState.Unregistred
        };
        Users.Add(newUser);
        return new SendMessageRequest(id, "Кто ты, воин?\n\nВведи свои данные в формате\nФамилия Имя");
    }
}