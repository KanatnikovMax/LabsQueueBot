using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

/// <summary>
/// Подписывает или отписывает пользователя от рассылки
/// </summary>
public class SwitchNotification : Command
{
    public override string Definition => "/switch_notification - Вкл/выкл ежедневное оповещение";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        var user = Users.At(id);
        user.IsNotifyNeeded = !user.IsNotifyNeeded;
        var textToSend = user.IsNotifyNeeded
            ? "Вы подписались на рассылку"
            : "Вы отписались от рассылки";
        return new SendMessageRequest(id, textToSend);
    }
}