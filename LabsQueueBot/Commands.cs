using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bots.Http;

namespace LabsQueueBot
{
    internal abstract class Command
    {
        public abstract SendMessageRequest Run(Update update);
        public abstract string Definition { get; }
    }

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
            Users.At(id).State = User.UserState.UnsetStudentData;
            return new SendMessageRequest(id, "Кто ты, воин?\nВведи свои данные в одну строчку через пробел в следующем формате:\n" +
                "Курс Группа Фамилия Имя");
        }
    }

    internal class StartApplier : Command
    {
        public override string Definition { get => "/start_applier"; }

        public override SendMessageRequest Run(Update update)
        {
            /*
                TODO: Добавить как выглядит сообщение для регистрации:
                Курс(byte) -> course
                Группа(byte) -> group
                Фамилия Имя(string) -> name
             */
            long id = update.Message.Chat.Id;
            byte course = 0;
            byte group = 0;
            // TODO: обработать ситуацию с отсылкой файла
            string[] data = update.Message.Text.ToString().Split(' ');
            Users.At(id).State = User.UserState.None;
            if (data.Length != 4 || !Byte.TryParse(data[0], out course) || !Byte.TryParse(data[1], out group) || data[2].Equals("") || data[3].Equals(""))
            {
                Users.Remove(id);
                return new SendMessageRequest(id, "Ты - ошибка, и жизнь твоя - ошибка");
            }
            try
            {
                User user = new User(course, group, $"{data[2]} {data[3]}", id);
                Users.Add(user);
            }
            catch(ArgumentException exception)
            {
                Users.Remove(id);
                return new SendMessageRequest(update.Message.Chat.Id, exception.Message);
            }
            //return new SendMessageRequest(id, "Вы успешно зарегистрированы");
            return new Help().Run(update);
        }
    }

    internal class Help : Command
    {
        public override string Definition { get => "/help"; }

        public override SendMessageRequest Run(Update update)
        {
            StringBuilder builder = new StringBuilder("список");
            return new SendMessageRequest(update.Message.Chat.Id, builder.ToString());
        }
    }

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

    internal class Skip : Command
    {
        public override string Definition { get => "/skip"; }

        public override SendMessageRequest Run(Update update)
        {

            // Вызвать /subjects и выбираешь предмет
            throw new NotImplementedException();
        }
    }

    internal class Quit : Command
    {
        public override string Definition { get => "/quit"; }

        public override SendMessageRequest Run(Update update)
        {

            throw new NotImplementedException();
        }
    }

    internal class Subjects : Command
    {
        public override string Definition { get => "/subjects"; }

        public override SendMessageRequest Run(Update update)
        {
            long id = update.Message.Chat.Id;
            string subjects = Groups.ShowSubjects(id);
            //return new SendMessageRequest(id, subjects);
            return new SendMessageRequest(id, subjects);
        }
    }

    internal class ChangeGroup : Command
    {
        public override string Definition { get => "/change_group"; }

        public override SendMessageRequest Run(Update update)
        {

            
            throw new NotImplementedException();
        }
    }

    internal class Join : Command
    {
        public override string Definition { get => "/join"; }

        public override SendMessageRequest Run(Update update)
        {
            throw new NotImplementedException();
        }
    }

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
