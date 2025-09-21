using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LabsQueueBot.Core.Helpers;

public static class BotClientUpdateHelper
{
    public static int? GetUpdateMessageId(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                return update.Message!.MessageId;
            
            case UpdateType.CallbackQuery:
                return update.CallbackQuery!.Message!.MessageId;
            
            default:
                return null;
        }
    }

    public static long GetUpdateChatId(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                return update.Message!.Chat.Id;
            
            case UpdateType.CallbackQuery:
                return update.CallbackQuery!.Message!.Chat.Id;
        }

        return -1;
    }
}