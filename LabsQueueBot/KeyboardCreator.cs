using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bots.Http;
using System.Threading;
using Telegram.Bot;
using Telegram.Bots.Extensions.Polling;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot
{
    static internal class KeyboardCreator
    {
        public static InlineKeyboardMarkup ListToKeyboardTemplate<T>(List<string> list, int n, int m) where T : IConvertible
        {

            //n = количество элементов в листе
            //m = желаемое количество элементов в строке выходной таблицы
            int size = n / m + (n % m != 0 ? 1 : 0); //значи size = количество строк в выходной таблице
            InlineKeyboardButton[][] arr = new InlineKeyboardButton[size + 1][];

            for (int i = 0; i < n / m; i++)
            {
                arr[i] = new InlineKeyboardButton[m];
                for (int j = 0; j < m; j++)
                {
                    arr[i][j] = InlineKeyboardButton.WithCallbackData(Convert.ToString(list[i * m + j]));
                }
            }
            if (n % m != 0)
            {
                arr[size - 1] = new InlineKeyboardButton[n % m];
                for (int i = 0; i < n % m; i++)
                {
                    arr[size - 1][i] = InlineKeyboardButton.WithCallbackData(Convert.ToString(list[(size - 1) * m + i]));
                }
            }
            arr[size] = new InlineKeyboardButton[2] { InlineKeyboardButton.WithCallbackData("Добавить"), InlineKeyboardButton.WithCallbackData("Назад") };
            return new InlineKeyboardMarkup(arr);
        }

        public static InlineKeyboardMarkup ListToKeyboard(List<string> list, bool isNeedAdd, bool isNeedBack, int collumnsCount)
        {
            int elementsCount = list.Count;
            int size = elementsCount / collumnsCount + (elementsCount % collumnsCount != 0 ? 1 : 0);
            //TODO: подумать над сортировкой выводимых данных
            //list.Sort();
            InlineKeyboardButton[][] arr = new InlineKeyboardButton[size + (isNeedAdd ? 1 : 0) + (isNeedBack ? 1 : 0)][];

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
                    arr[size - 1][i] = InlineKeyboardButton.WithCallbackData(Convert.ToString(list[(size - 1) * collumnsCount + i]));
                }
            }


            if (isNeedAdd)
                arr[size] = new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Добавить") };
            if (isNeedBack)
                arr[size + (isNeedAdd ? 1 : 0)] = new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Назад") };



            //if (isNeedAdd)
            //{
            //    arr[size] = new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Добавить") };
            //    if (isNeedBack)
            //        arr[size + 1] = new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Назад") };
            //}
            //else 
            //    if (isNeedBack)
            //        arr[size] = new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Назад") };

            return new InlineKeyboardMarkup(arr);
        }
    }
}
