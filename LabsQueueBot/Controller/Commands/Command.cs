using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;


namespace LabsQueueBot
{
    /// <summary>
    /// Общий класс для определения ответных действий на конкретный update пользователя
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Инициирует ответные действия на запрос update
        /// </summary>
        public abstract SendMessageRequest Run(Update update);
        /// <summary>
        /// Возвращает InlineKeyboardMarkup, если это предполагает ответ на запрос
        /// </summary>
        /// <returns>
        /// InlineKeyboardMarkup или null
        /// </returns>
        public abstract InlineKeyboardMarkup? GetKeyboard(Update update);
        public abstract string Definition { get; }
    }
}
