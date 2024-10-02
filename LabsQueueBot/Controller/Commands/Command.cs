using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;


namespace LabsQueueBot
{
    public abstract class Command
    {
        public abstract SendMessageRequest Run(Update update);
        public abstract InlineKeyboardMarkup? GetKeyboard(Update update);
        public abstract string Definition { get; }
    }
}
