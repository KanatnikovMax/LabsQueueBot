using System.Runtime.InteropServices.ComTypes;
using LabsQueueBot.Model;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot.Controller.Commands.Appliers;

public class BanApplier : Command
{
    public override string Definition => "/ban_applier";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        var subject = update.CallbackQuery.Data;
        var id = update.CallbackQuery.Message.Chat.Id;
        
        

        if (update.CallbackQuery.Message.Text != "Выберите очередь по предмету, из которой необходимо забанить пользователя:")
            throw new InvalidOperationException();

        //отмена бана
        if (subject == "Назад")
            return new SendMessageRequest(id, "Предмет не выбран, пользователь не будет забанен");

        //добавление пользователя в черный список по выбранному предмету
        var userToBanId = Groups.bans[id];
        Groups.bans.Remove(id);

        var user = Users.At(id);
        var group = Groups.At(new GroupKey(user.CourseNumber, user.GroupNumber));
        group.AddToBlackListBySubject(subject, userToBanId);

        return new SendMessageRequest(id, "Выбранный пользователь сможет встать очередь только после ближайшей рандомизации");
    }
}