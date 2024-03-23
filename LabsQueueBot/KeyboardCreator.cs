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
        public static InlineKeyboardMarkup ListToKeyboard(List<string> list, bool isNeedAdd, bool isNeedBack, int collumnsCount)
        {
            
            int elementsCount = list.Count;
            int size = elementsCount / collumnsCount + (elementsCount % collumnsCount != 0 ? 1 : 0);
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

                
            
            return new InlineKeyboardMarkup(arr);
        }
    }
}
