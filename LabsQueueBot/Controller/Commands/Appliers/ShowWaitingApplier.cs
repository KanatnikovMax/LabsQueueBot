using LabsQueueBot.Model;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot.Controller.Commands.Appliers;

public class ShowWaitingApplier : Command
{
    public override string Definition => "/show_waiting_applier";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        var subject = update.CallbackQuery.Data;
        var id = update.CallbackQuery.Message.Chat.Id;

        if (update.CallbackQuery.Message.Text != "Выберите предмет:")
            throw new InvalidOperationException();

        if (subject == "Назад")
            return new SendMessageRequest(id, subject);

        return new SendMessageRequest(id, Groups.ShowWaiting(id, subject));
    }
}