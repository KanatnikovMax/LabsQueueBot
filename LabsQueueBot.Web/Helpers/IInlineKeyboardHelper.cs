using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot.Web.Helpers;

public static class InlineKeyboardHelper
{
    public static readonly string AddMessage = "Добавить";
    public static readonly string BackMessage = "Отмена";

    /// <summary>
    /// Создает InlineKeyboardMarkup по заданным параметрам
    /// </summary>
    /// <param name="list"> список строковых значений, отображаемых в InlineKeyboardMarkup </param>
    /// <param name="isNeedAdd"> необходимость кнопки "add" </param>
    /// <param name="isNeedBack"> необходимость кнопки "back" </param>
    /// <param name="columnCount"> количество колонок в InlineKeyboardMarkup </param>
    /// <returns></returns>
    public static InlineKeyboardMarkup ListToKeyboard(List<string> list, bool isNeedAdd, bool isNeedBack, int columnCount)
    {
        var elementsCount = list.Count;
        var size = elementsCount / columnCount + (elementsCount % columnCount != 0 ? 1 : 0);
        InlineKeyboardButton[][] arr =
            new InlineKeyboardButton[size + (isNeedAdd ? 1 : 0) + (isNeedBack ? 1 : 0)][];

        for (var i = 0; i < elementsCount / columnCount; i++)
        {
            arr[i] = new InlineKeyboardButton[columnCount];
            for (var j = 0; j < columnCount; j++)
            {
                arr[i][j] = InlineKeyboardButton.WithCallbackData(Convert.ToString(list[i * columnCount + j]));
            }
        }

        if (elementsCount % columnCount != 0)
        {
            arr[size - 1] = new InlineKeyboardButton[elementsCount % columnCount];
            for (var i = 0; i < elementsCount % columnCount; i++)
            {
                arr[size - 1][i] =
                    InlineKeyboardButton.WithCallbackData(Convert.ToString(list[(size - 1) * columnCount + i]));
            }
        }

        if (isNeedAdd)
            arr[size] = [InlineKeyboardButton.WithCallbackData(AddMessage)];
        if (isNeedBack)
            arr[size + (isNeedAdd ? 1 : 0)] = [InlineKeyboardButton.WithCallbackData(BackMessage)];


        return new InlineKeyboardMarkup(arr);
    }
}