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
            arr[size] = new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Назад") };
            return new InlineKeyboardMarkup(arr);
        }

        public static InlineKeyboardMarkup ListToKeyboard(List<string> list, int size)
        {
            InlineKeyboardButton[][] arr = new InlineKeyboardButton[size + 1][];
            for (int i = 0; i < size; ++i)
            {
                arr[i] = new InlineKeyboardButton[1];
                arr[i][1] = InlineKeyboardButton.WithCallbackData(list[i]);
            }
            
            arr[size] = new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("Назад") };
            return new InlineKeyboardMarkup(arr);
        }
    }


    internal abstract class Command
    {
        public abstract SendMessageRequest Run(Update update);
        public abstract string Definition { get; }
    }

    //начать работу с ботом
    internal class Start : Command
    {
        public override string Definition { get => "/start"; }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            if (Users.Contains(id))
            {
                return new SendMessageRequest(id, "Ты уже зареган\nИди отсюда, розбийник");
            }    
            Users.Add(new User(id));
            Users.At(id).State = User.UserState.Unregistred;
            return new SendMessageRequest(id, "Кто ты, воин?\n\nВведи свои данные в формате\nФамилия Имя");
        }
    }

    //создание пользователя(id, name) в мапе
    internal class StartApplier : Command
    {
        public override string Definition { get => "/start_applier"; }

        public override SendMessageRequest Run(Update update)
        {
            /*
                TODO: Добавить меню регистрации в группу и создания новой группы 
                
             */
            long id = update.Message.Chat.Id;
            // TODO: обработать ситуацию с отсылкой файла
            string[] data = update.Message.Text.ToString().Split(' ');
            Users.At(id).State = User.UserState.None;
            if (data.Length != 2 || data[0].Equals("") || data[1].Equals(""))
            {
                Users.Remove(id);
                return new SendMessageRequest(id, "Твое имя - ошибка, и жизнь твоя - ошибка");
            }
            try
            {
                Users.Add(id, $"{data[0]} {data[1]}");
                Users.At(id).State = User.UserState.UnsetStudentData;
            }
            catch(ArgumentException exception)
            {
                Users.Remove(id);
                return new SendMessageRequest(update.Message.Chat.Id, exception.Message);
            }
            
            return new SetGroup().Run(update);
        }
    }

    //очевидно, помощь
    internal class Help : Command
    {
        public override string Definition { get => "/help"; }

        public override SendMessageRequest Run(Update update)
        {
            StringBuilder builder = new StringBuilder("список");
            return new SendMessageRequest(update.Message.Chat.Id, builder.ToString());
        }
    }

    //отписаться от бота
    internal class Stop : Command
    {
        public override string Definition { get => "/stop"; }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            // TODO: убрать из всех очередей
            Groups.Remove(id);

            Users.Remove(id);
            return new SendMessageRequest(id, "Вы разлогинены");
        }
    }

    //пропустить человека вперед себя
    internal class Skip : Command
    {
        public override string Definition { get => "/skip"; }

        public override SendMessageRequest Run(Update update)
        {

            // Вызвать /subjects и выбираешь предмет
            throw new NotImplementedException();
        }
    }

    //выйти из очереди
    internal class Quit : Command
    {
        public override string Definition { get => "/quit"; }

        public override SendMessageRequest Run(Update update)
        {

            throw new NotImplementedException();
        }
    }

    //показывает предметы и номера в очереди по ним
    internal class Subjects : Command
    {
        public override string Definition { get => "/subjects"; }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            return new SendMessageRequest(id, Groups.ShowSubjects(id));
        }
    }

    //общий класс для изменеия курса и группы
    internal class SetGroup : Command
    {
        public override string Definition { get => "/change_group"; }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;

            //TODO: проверка что не превышаем количество людей в группе
            Users.At(id).State = User.UserState.None;
            return new SendMessageRequest(id, $"Вызван {Definition}");
        }
    }

    //встать в очередь
    internal class Join : Command
    {
        public override string Definition { get => "/join"; }

        public override SendMessageRequest Run(Update update)
        {
            throw new NotImplementedException();
        }
    }

    //показывает очереди по предметам
    internal class Show : Command
    {
        public override string Definition { get => "/show"; }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            SendMessageRequest request;
            try
            {
                request = new Subjects().Run(update);
                request = new SendMessageRequest(id, new StringBuilder(request.Text).AppendLine("Введите название предмета").ToString());
                Users.At(id).State = User.UserState.ShowQueue;
            }
            catch(InvalidDataException)
            {
                request = new SendMessageRequest(id, "Твои данные - ошибка, и жизнь твоя - ошибка");
            }
            catch (Exception)
            {
                request = new SendMessageRequest(id, "Твои данные - ошибка, и жизнь твоя - ошибка");
            }
            return request;
        }
    }

    //вызывает меню с предметами
    internal class ShowApplier : Command
    {
        public override string Definition { get => "/show_applier"; }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            string subject = update.Message.Text;


            throw new NotImplementedException();
        }
    }
}
