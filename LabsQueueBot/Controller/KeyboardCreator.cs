using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot
{
    /// <summary>
    /// Класс для создания ответов InlineKeyboardMarkup
    /// </summary>
    static public class KeyboardCreator
    {
        /// <summary>
        /// Создает InlineKeyboardMarkup по заданным параметрам
        /// </summary>
        /// <param name="list"> список строковых значений, отображаемых в InlineKeyboardMarkup </param>
        /// <param name="isNeedAdd"> необходимость кнопки "add" </param>
        /// <param name="isNeedBack"> необходимость кнопки "back" </param>
        /// <param name="collumnsCount"> количество колонок в InlineKeyboardMarkup </param>
        /// <returns></returns>
        public static InlineKeyboardMarkup ListToKeyboard(List<string> list, bool isNeedAdd, bool isNeedBack,
            int collumnsCount)
        {
            int elementsCount = list.Count;
            int size = elementsCount / collumnsCount + (elementsCount % collumnsCount != 0 ? 1 : 0);
            InlineKeyboardButton[][]
                arr = new InlineKeyboardButton[size + (isNeedAdd ? 1 : 0) + (isNeedBack ? 1 : 0)][];

            for (int i = 0; i < elementsCount / collumnsCount; i++)
            {
                arr[i] = new InlineKeyboardButton[collumnsCount];
                for (int j = 0; j < collumnsCount; j++)
                {
                    arr[i][j] = InlineKeyboardButton.WithCallbackData(Convert.ToString(list[i * collumnsCount + j]));
                }
            }

            if (elementsCount % collumnsCount != 0)
            {
                arr[size - 1] = new InlineKeyboardButton[elementsCount % collumnsCount];
                for (int i = 0; i < elementsCount % collumnsCount; i++)
                {
                    arr[size - 1][i] =
                        InlineKeyboardButton.WithCallbackData(Convert.ToString(list[(size - 1) * collumnsCount + i]));
                }
            }

            if (isNeedAdd)
                arr[size] = new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Добавить") };
            if (isNeedBack)
                arr[size + (isNeedAdd ? 1 : 0)] = new InlineKeyboardButton[1]
                    { InlineKeyboardButton.WithCallbackData("Назад") };


            return new InlineKeyboardMarkup(arr);
        }
    }
}