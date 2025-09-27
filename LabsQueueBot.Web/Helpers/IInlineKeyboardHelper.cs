using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot.Web.Helpers;

public static class InlineKeyboardHelper
{
    public static readonly string AddMessage = "Добавить";
    public static readonly string BackMessage = "Отмена";

    /// <summary>
    /// Создает InlineKeyboardMarkup по заданным параметрам
    /// </summary>
    /// <param name="valuesList"> список строковых значений, отображаемых в таблице </param>
    /// <param name="columnCount"> количество колонок в таблице </param>
    /// <param name="isNeedAdd"> флаг необходимости кнопки "Добавить", по умолчанию false </param>
    /// <param name="isNeedBack"> флаг необходимости кнопки "Отмена", по умолчанию true </param>
    /// <returns>таблица InlineKeyboardMarkup</returns>
    public static InlineKeyboardMarkup ListToKeyboard(List<string> valuesList, int columnCount, bool isNeedAdd = false, bool isNeedBack = true)
    {
        var elementsCount = valuesList.Count;
        var size = elementsCount / columnCount + (elementsCount % columnCount != 0 ? 1 : 0);
        var arr = new InlineKeyboardButton[size + (isNeedAdd ? 1 : 0) + (isNeedBack ? 1 : 0)][];

        for (var i = 0; i < elementsCount / columnCount; i++)
        {
            arr[i] = new InlineKeyboardButton[columnCount];
            for (var j = 0; j < columnCount; j++)
            {
                // arr[i][j] = InlineKeyboardButton.WithCallbackData(Convert.ToString(valuesList[i * columnCount + j]));
                arr[i][j] = InlineKeyboardButton.WithCallbackData(valuesList[i * columnCount + j]);
            }
        }

        if (elementsCount % columnCount != 0)
        {
            arr[size - 1] = new InlineKeyboardButton[elementsCount % columnCount];
            for (var i = 0; i < elementsCount % columnCount; i++)
            {
                // arr[size - 1][i] = InlineKeyboardButton.WithCallbackData(Convert.ToString(valuesList[(size - 1) * columnCount + i]));
                arr[size - 1][i] = InlineKeyboardButton.WithCallbackData(valuesList[(size - 1) * columnCount + i]);
            }
        }

        if (isNeedAdd)
            arr[size] = [InlineKeyboardButton.WithCallbackData(AddMessage)];
        if (isNeedBack)
            arr[size + (isNeedAdd ? 1 : 0)] = [InlineKeyboardButton.WithCallbackData(BackMessage)];


        return new InlineKeyboardMarkup(arr);
    }
}