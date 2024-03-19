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
    internal abstract class Command 
    {
        public abstract SendMessageRequest Run(Update update);
        public abstract InlineKeyboardMarkup? GetKeyboard(Update update);
        public abstract string Definition { get; }
    }

    //начать работу с ботом
    internal class Start : Command
    {
        public override string Definition { get => "/start"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }

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

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return new SetGroup().GetKeyboard(update);
        }

        public override SendMessageRequest Run(Update update)
        {
            
            long id = update.Message.Chat.Id;

            var data = update.Message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            Users.At(id).State = User.UserState.None;
            if (data.Length != 2)
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
            return new SendMessageRequest(id, "Выберете свои курс и группу:");
        }
    }

    //очевидно, помощь
    internal class Help : Command
    { 
        public override string Definition { get => "/help - Список команд"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }

        public override SendMessageRequest Run(Update update)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("\n(Описание блока с /help)");
            builder.AppendLine(new Help().Definition);

            builder.AppendLine("\nДействия с очередями");
            builder.AppendLine(new Subjects().Definition);
            builder.AppendLine(new ShowQueue().Definition);
            builder.AppendLine(new Join().Definition);
            builder.AppendLine(new Quit().Definition);
            builder.AppendLine(new Skip().Definition);

            //TODO: может добавить возможность смены имени?
            builder.AppendLine("\nИзменить информацию о себе");
            builder.AppendLine("(change_name - команда для смены имени)");
            builder.AppendLine(new SetGroup().Definition);

            builder.AppendLine("(Описание блока с /stop)");
            builder.AppendLine(new Stop().Definition);

            return new SendMessageRequest(update.Message.Chat.Id, builder.ToString());
        }
    }

    //отписаться от бота
    internal class Stop : Command
    {
        public override string Definition { get => "/stop - Отписаться"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }

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
        public override string Definition { get => "/skip - Пропустить одного человека вперёд себя"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return new Show().GetKeyboard(update);
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            Users.At(id).State = User.UserState.Skip;
            return new SendMessageRequest(id, "Выберете предмет");
        }
    }

    internal class SkipApplier : Command
    {
        public override string Definition => "/skip_applier";

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }

        public override SendMessageRequest Run(Update update)
        {
            throw new NotImplementedException();
        }
    }

    //выйти из очереди
    internal class Quit : Command
    {
        public override string Definition { get => "/quit - Выйти из очереди"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return new Show().GetKeyboard(update);
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            Users.At(id).State = User.UserState.Quit;
            return new SendMessageRequest(id, "Выберите предмет:");
        }
    }

    internal class QuitApplier : Command
    {
        public override string Definition { get => "/quit_applier"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            throw new NotImplementedException();
        }

        public override SendMessageRequest Run(Update update)
        {
            throw new NotImplementedException();
        }
    }


    //показывает предметы и номера в очереди по ним
    internal class Subjects : Command
    {
        public override string Definition { get => "/subjects - Список предметов и номера в очередях по ним"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            
            return null;
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            return new SendMessageRequest(id, Groups.ShowSubjects(id));
        }
    }

    //общий класс для изменения курса и группы
    internal class SetGroup : Command
    {
        public override string Definition { get => "/change_info - Изменить номера курса и  группы"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            long id = update.Message.Chat.Id;
            if (!Users.Contains(id))
                return null;
            var list = Groups.Keys.Select(x => x.ToString()).ToList();
            bool flag = Users.At(id).State != User.UserState.UnsetStudentData; // нужна ли кнопка "Назад"
            return KeyboardCreator.ListToKeyboard(list, flag, 1);
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            return new SendMessageRequest(id, "Выберете свои курс и группу:");
        }
    }

    // TODO: доделать
    internal class SetGroupApplier : Command
    {
        public override string Definition => "/set_group_applier";

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }

        public override SendMessageRequest Run(Update update)
        {
            var text = update.CallbackQuery.Data;
            var id = update.CallbackQuery.Message.Chat.Id;
            if (text == "Назад")
                return new SendMessageRequest(id, text);
            //TODO: добавление пользователя в группу
            if (text != "Добавить")
            {
                var line = text.Split(' ');
                byte course = Convert.ToByte(line[0]);
                byte group = Convert.ToByte(line[2]);
                if (!Groups.At(new GroupKey(course, group)).AddStudent())
                    return new SendMessageRequest(id, "Много");

                Users.Add(new User(course, group, Users.At(id).Name, id));
                return new SendMessageRequest(id, $"Вызван {Definition}");
            }
            // TODO: на кнопку добавить появляется новая группа

            return new SendMessageRequest(id, "Создана новая группа");
        }
    }

    //встать в очередь
    internal class Join : Command
    {
        public override string Definition { get => "/join - Встать в очередь"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return new Show().GetKeyboard(update);
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            Users.At(id).State = User.UserState.Join;
            return new SendMessageRequest(id, "Выберете предмет");
        }
    }

    internal class JoinApplier : Command
    {
        public override string Definition { get => "/join_applier"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }
        public override SendMessageRequest Run(Update update)
        {
            var subject = update.CallbackQuery.Data;
            var id = update.CallbackQuery.Message.Chat.Id;

            if (subject == "Назад")
                return new SendMessageRequest(id, subject);

            User user = Users.At(id);
            Group group = Groups.At(new GroupKey(user.Course, user.Group));

            if (subject == "Добавить" && !group.AddSubject(subject))
                return new SendMessageRequest(id, "Много");

            group.AddStudent(id, subject);
            return new SendMessageRequest(id, $"Твой номер в очереди — {group[subject].Position(id) + 1}");
        }
    }


    //показывает очереди по предметам
    internal class Show : Command
    {
        public override string Definition { get => "/show"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            User user = Users.At(update.Message.Chat.Id);
            var gr = Groups.groups[new GroupKey(2, 9)]._Keys();
            List<string> subjects = Groups.groups[new GroupKey(user.Course, user.Group)].Keys.ToList();
            return KeyboardCreator.ListToKeyboard(subjects, true, 1);
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            //SendMessageRequest request;
            //try
            //{
            //    request = new Subjects().Run(update);
            //    request = new SendMessageRequest(id, new StringBuilder(request.Text).AppendLine("Введите название предмета").ToString());
            //    Users.At(id).State = User.UserState.ShowQueue;
            //}
            //catch(InvalidDataException)
            //{
            //    request = new SendMessageRequest(id, "Твои данные - ошибка, и жизнь твоя - ошибка");
            //}
            //catch (Exception)
            //{
            //    request = new SendMessageRequest(id, "Твои данные - ошибка, и жизнь твоя - ошибка");
            //}


            //var keyboard = KeyboardCreator.ListToKeyboardTemplate<string>(subjects, subjects.Count, 1);
            //var keyboard = KeyboardCreator.ListToKeyboard(subjects, true, 1);
            //return new SendMessageRequest(id,subjects);
            Users.At(id).State = User.UserState.ShowQueue;
            return new SendMessageRequest(id, "Выберите предмет:");
        }
    }

    //вызывает меню с предметами
    internal class ShowApplier : Command
    {
        public override string Definition { get => "/show_applier"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            string subject = update.Message.Text;
            


            return new SendMessageRequest(id, "");
        }
    }

    internal class ShowQueue : Command
    {
        public override string Definition => "/show - Показать очередь полностью";

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            throw new NotImplementedException();
        }

        public override SendMessageRequest Run(Update update)
        {
            throw new NotImplementedException();
        }
    }
    internal class ShowQueueApplier : Command
    {
        public override string Definition => throw new NotImplementedException();

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            throw new NotImplementedException();
        }

        public override SendMessageRequest Run(Update update)
        {
            throw new NotImplementedException();
        }
    }

}

