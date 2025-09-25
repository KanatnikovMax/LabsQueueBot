using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LabsQueueBot.Core.Extensions;

public static class UpdateExtensions
{
    public static bool IsValid(this Update update)
    {
        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
            return true;

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery!.Data != null && !update.CallbackQuery!.IsGameQuery)
            return true;
        
        List<ChatMemberStatus> leftChat = [ChatMemberStatus.Member, ChatMemberStatus.Restricted];
        if (update.Type == UpdateType.MyChatMember && leftChat.Contains(update.MyChatMember!.OldChatMember.Status))
            return true;

        return false;
    }
    
    public static bool IsValidMessage(this Update update)
        => update.Message!.Type == MessageType.Text && update.Message.Text != null;
    
    public static bool IsValidCallbackQuery(this Update update, int? messageId)
        => messageId != null && update.CallbackQuery!.Message!.MessageId == messageId;

    public static bool IsValidMyChatMember(this Update update)
    {
        List<ChatMemberStatus> leftChat = [ChatMemberStatus.Left, ChatMemberStatus.Kicked];
        return leftChat.Contains(update.MyChatMember!.NewChatMember.Status);
    }
    
    /// <summary>
    /// Метод расширения для получения MessageId пришедшего Update
    /// </summary>
    /// <returns>
    /// messageId, если Update.Type является UpdateType.Message или UpdateType.CallbackQuery; <br/>
    /// иначе null
    /// </returns>
    public static int? GetMessageId(this Update update)
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

    /// <summary>
    /// Метод расширения для получения ChatId пришедшего Update
    /// </summary>
    /// <returns>
    /// chatId, если Update.Type является UpdateType.Message, или UpdateType.CallbackQuery, или UpdateType.MyChatMember; <br/>
    /// иначе null
    /// </returns>
    public static long? GetChatId(this Update update)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                return update.Message!.Chat.Id;
            
            case UpdateType.CallbackQuery:
                return update.CallbackQuery!.Message!.Chat.Id;
            
            case UpdateType.MyChatMember:
                return update.MyChatMember!.Chat.Id;
            
            default:
                return null;
        }
    }

    public static bool IsTextMessage(this Update update, string text)
        => update.Type == UpdateType.Message && update.Message!.Text == text;
    
    public static bool IsCommand(this Update update)
        => update.Message!.Text!.StartsWith('/');
}