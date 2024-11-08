﻿using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

/// <summary>
/// Класс для изменения курса и группы
/// </summary>
public class SetGroup : Command
{
    public override string Definition => "/change_group - Изменить номера курса и группы";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        long id = update.Message.Chat.Id;
        if (!Users.Contains(id))
            return null;
        var list = Groups.Keys.Select(x => x.ToString()).Order().ToList();
        bool backFlag = Users.At(id).State != User.UserState.UnsetStudentData; // нужна ли кнопка "Назад"
        if (backFlag)
            Users.At(id).State = User.UserState.ChangeData;
        bool addFlag = Groups.GroupsCount < 60;
        return KeyboardCreator.ListToKeyboard(list, addFlag, backFlag, 1);
    }

    public override SendMessageRequest Run(Update update)
    {
        long id = update.Message.Chat.Id;
        return new SendMessageRequest(id, "Выберите свои курс и группу:");
    }
}