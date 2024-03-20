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
using System.Linq.Expressions;


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
            catch (ArgumentException exception)
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
            long id;
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                id = update.Message.Chat.Id;
            else
                id = update.CallbackQuery.Message.Chat.Id;

            StringBuilder builder = new StringBuilder();

            builder.AppendLine("\n(Описание блока с /help)");
            builder.AppendLine(new Help().Definition);

            builder.AppendLine("\nДействия с очередями");
            builder.AppendLine(new Subjects().Definition);
            builder.AppendLine(new ShowQueue().Definition);
            builder.AppendLine(new Join().Definition);
            builder.AppendLine(new Quit().Definition);
            builder.AppendLine(new Skip().Definition);

            builder.AppendLine("\nИзменить информацию о себе");
            builder.AppendLine(new Rename().Definition);
            builder.AppendLine(new SetGroup().Definition);

            builder.AppendLine("(Описание блока с /stop)");
            builder.AppendLine(new Stop().Definition);

            return new SendMessageRequest(id, builder.ToString());
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
            Groups.Remove(id);
            Users.Remove(id);
            return new SendMessageRequest(id, "Прощай, мой друг");
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
            var subject = update.CallbackQuery.Data;
            var id = update.CallbackQuery.Message.Chat.Id;

            if (subject == "Назад")
                return new SendMessageRequest(id, subject);

            User user = Users.At(id);
            Group group = Groups.At(new GroupKey(user.Course, user.Group));
            try
            {
                group[subject].Skip(id);
                return new SendMessageRequest(id, "Это как шаг вперед, но назад");
            }
            catch (InvalidOperationException exception)
            {
                return new SendMessageRequest(id, exception.Message);
            }
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

            if (group.RemoveStudentFromQueue(id, subject))
                return new SendMessageRequest(id, "Ты вышел из очереди");

            return new SendMessageRequest(id, "Ты не можешь выйти из очереди, в которой не числишься");
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
            bool backFlag = Users.At(id).State != User.UserState.UnsetStudentData; // нужна ли кнопка "Назад"
            if(backFlag)
                Users.At(id).State = User.UserState.ChangeData;
            bool addFlag = Groups.GroupsCount < 50;
            return KeyboardCreator.ListToKeyboard(list, addFlag, backFlag, 1);
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
                if(Users.At(id).State == User.UserState.ChangeData)
                    Groups.Remove(id);
                if (!Groups.At(new GroupKey(course, group)).AddStudent())
                {


                    return new SendMessageRequest(id, "Много");
                }
                Users.Add(new User(course, group, Users.At(id).Name, id));
                Users.At(id).State = User.UserState.None;
                return new Help().Run(update);
                //return new SendMessageRequest(id, $"Вы были зачислены на {Definition} ");
            }
            else
                Users.At(id).State = User.UserState.AddGroup;
            // TODO: на кнопку добавить появляется новая группа
            return new SendMessageRequest(id, "Введите свои курс и группу. Например, для n курса m группы\nn:m");
        }
    }

    internal class AddGroupApplier : Command
    {
        public override string Definition => "/add_group_applier";

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            string[] text = update.Message.Text.Split(':');

            byte course;
            byte group;

            if (text.Length != 2 || !Byte.TryParse(text[0], out course) || !Byte.TryParse(text[1], out group))
                return new SendMessageRequest(id, "Некорректные данные\nПовторите ввод");

            if (course == Users.At(id).Course && group == Users.At(id).Group)
                return new SendMessageRequest(id, "Ты итак в этой группе, дурачок\nПовтори ввод");

            StringBuilder builder = new StringBuilder();
            
            try
            {
                Groups.Remove(id);
                Groups.Add(course, group);

                Users.Add(new User(course, group, Users.At(id).Name, id));
                Users.At(id).State = User.UserState.None;
                Groups.At(new GroupKey(course, group)).AddStudent();

                builder.AppendLine($"Вы были добавлены в {course} курс {group} группу");
            }
            catch (ArgumentException exception)
            {
                builder.AppendLine(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                Users.Add(new User(course, group, Users.At(id).Name, id));
                Users.At(id).State = User.UserState.None;
                Groups.At(new GroupKey(course, group)).AddStudent();

                builder.AppendLine(exception.Message);
                builder.AppendLine($"Вы были добавлены в {course} курс {group} группу");
            }

            return new SendMessageRequest(id, builder.ToString());
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

            if (subject == "Добавить" /*&& !group.AddSubject(subject)*/)
            {
                user.State = User.UserState.AddSubject;
                return new SendMessageRequest(id, "Введите название нового предмета");
            }
            if (group.AddStudent(id, subject))
                return new SendMessageRequest(id, $"Твой номер в очереди — {group[subject].Position(id) + 1}");
            return new SendMessageRequest(id, "Ты уже записан в эту очередь");
        }
    }

    internal class AddSubjectApplier : Command
    {
        public override string Definition { get => "/add_subject_applier"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            string subject = update.Message.Text.Trim();
            User user = Users.At(id);
            Group group = Groups.At(new GroupKey(user.Course, user.Group));
            user.State = User.UserState.None;
            try
            {
                group.AddSubject(subject);
                // TODO: решить что делать с добавлением студента в очередь сразу после создания
                return new SendMessageRequest(id, $"Очередь по предмету {subject} добавлена");
            }
            catch (Exception exception)
            {
                return new SendMessageRequest(id, exception.Message);
            }
            
        }
    }

    //показывает очереди по предметам
    internal class Show : Command
    {
        public override string Definition { get => "/show"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            long id = update.Message.Chat.Id;
            User user = Users.At(update.Message.Chat.Id);
            var gr = Groups.groups[new GroupKey(2, 9)]._Keys();
            List<string> subjects = Groups.groups[new GroupKey(user.Course, user.Group)].Keys.ToList();
            bool addFlag = Users.At(id).State == User.UserState.Join;
            return KeyboardCreator.ListToKeyboard(subjects, addFlag, true, 1);
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
        public override string Definition { get => "/show - Показать очередь полностью"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return new Show().GetKeyboard(update);
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            Users.At(id).State = User.UserState.ShowQueue;
            return new SendMessageRequest(id, "Выберете предмет:");
        }
    }

    internal class ShowQueueApplier : Command
    {
        public override string Definition => "/show_queue_applier";

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
            if (subject == "Добавить")
            {
                user.State = User.UserState.AddSubject;
                return new SendMessageRequest(id, "Введите название нового предмета:");
            }

            Group group = Groups.At(new GroupKey(user.Course, user.Group));
            //var queue = group[subject];
            //var builder = new StringBuilder();
            //int i = 1;
            //foreach (var student in queue)
            //{
            //    builder.AppendLine($"{i}. {student.Name}");
            //    ++i;
            //}
            return new SendMessageRequest(id, Groups.ShowQueue(id, subject));
        }
    }

    internal class Rename : Command
    {
        public override string Definition { get => "/rename - Смена фамилии и имени"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            Users.At(id).State = User.UserState.Rename;
            return new SendMessageRequest(id, "Кто таков будешь?\n(фамилия, имя)");
        }
    }

    internal class RenameApplier : Command
    {
        public override string Definition { get => "/rename_applier"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            var data = update.Message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            Users.At(id).State = User.UserState.None;
            if (data.Length != 2)
            {
                return new SendMessageRequest(id, "Смена личности не удалась");
            }
            try
            {
                //Users.Add(id, $"{data[0]} {data[1]}");
                Users.At(id).Name = $"{data[0]} {data[1]}";
                //Users.At(id).State = User.UserState.None;
            }
            catch (ArgumentException exception)
            {
                return new SendMessageRequest(update.Message.Chat.Id, exception.Message);
            }
            return new SendMessageRequest(id, "Смена личности завершена");
        }


    }
    internal class Mult : Command
    {
        public override string Definition { get => "/Mult - Смена фамилии и имени"; }

        public override InlineKeyboardMarkup? GetKeyboard(Update update)
        {
            return null;
        }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            Users.At(id).State = User.UserState.Rename;
            return new SendMessageRequest(id, "wdaad");
        }

        //public override SendMessageRequest animation(Update update)
        //{

        //}
    }
}
