using LabsQueueBot.Core.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LabsQueueBot.Core.Utils;

public static class BotClientUtils
{
    public static async Task ClearMarkupMessage(ITelegramBotClient botClient, long chatId, int messageId, string message, CancellationToken cancellationToken)
    {
        await botClient.EditMessageReplyMarkupAsync(
            chatId: chatId,
            messageId: messageId,
            replyMarkup: null, // изменяем сообщение чтобы нельзя было ткнуть на кнопку
            cancellationToken: cancellationToken);
        await botClient.EditMessageTextAsync(
            chatId: chatId,  
            messageId: messageId,
            text: message, // изменяем сообщение чтобы показывался результат выбора
            cancellationToken: cancellationToken);
    }

    public static async Task<bool> DeleteUpdate(ITelegramBotClient botClient, long chatId, Update update, CancellationToken cancellationToken)
    {
        var messageId = update.GetMessageId();
        if (messageId == null)
            return false;
        
        await botClient.DeleteMessageAsync(
            chatId: chatId,
            messageId: messageId.Value,
            cancellationToken: cancellationToken);
        return true;
    }
}