using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bots.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using System.Text;
using Telegram.Bot.Requests;
using System.Diagnostics.Contracts;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bots.Http;
using System.Security.Cryptography;

namespace LabsQueueBot
{
    //TODO: подключить б/д и продумать логирование
    class Program
    {
        internal static readonly Dictionary<string, Command> commands = new()
        {
            {"/start", new Start() },
            {"/stop", new Stop() },
            {"/help", new Help() },
            {"/join", new Join() },
            {"/quit", new Quit() },
            {"/skip", new Skip() },
            {"/change_info", new SetGroup() },
            {"/subjects", new Subjects() },
            {"/show", new Show() },
            {"/rename", new Rename() },
            {"/mult", new Mult() }
        };



        internal static readonly Dictionary<User.UserState, Command> actions = new()
        {
            {User.UserState.Unregistred, new StartApplier() },
            {User.UserState.UnsetStudentData, new SetGroupApplier() },
            {User.UserState.ChangeData, new SetGroupApplier() },
            {User.UserState.Join, new JoinApplier() },
            {User.UserState.Quit, new QuitApplier() },
            {User.UserState.Skip, new SkipApplier() },
            {User.UserState.ShowQueue, new ShowQueueApplier() },
            {User.UserState.AddSubject, new AddSubjectApplier() },
            {User.UserState.AddGroup, new AddGroupApplier() },
            {User.UserState.Rename, new RenameApplier() }
        };

        static ITelegramBotClient bot = new TelegramBotClient("7098667146:AAHlUf4Y-cmOtkOmCcvFDVnKFHbkVlCgpJE");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            //return;
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));


            //RemoveInactiveUsers(null);
            //update.ChatMember.
            long id = 0;
            Message message;
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:

                    message = update.CallbackQuery.Message;
                    id = message.Chat.Id;
                    break;

                case Telegram.Bot.Types.Enums.UpdateType.Message:

                    message = update.Message;
                    id = message.Chat.Id;
                    //if (message.Type == Telegram.Bot.Types.Enums.MessageType.ChatMemberLeft)
                    //{
                    //    //await bot.Del
                    //    new Stop().Run(update);
                    //    return;
                    //}
                    if (update.Message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
                    {
                        string request;
                        if (Users.Contains(id) && Users.At(id).State != User.UserState.None)
                            request = "Пришли данные текстом или нажми на кнопку (в зависимости от ситуации)";
                        else
                            request = "Не принимаю данные такого типа, лошара";
                        await botClient.SendTextMessageAsync(message.Chat, request);
                        await botClient.DeleteMessageAsync(chatId: id, messageId: message.MessageId);
                        return;
                    }
                    break;

                case Telegram.Bot.Types.Enums.UpdateType.MyChatMember:

                    id = update.MyChatMember.Chat.Id;
                    message = null;
                    if(Users.Contains(id))
                        Groups.Remove(id);
                        Users.Remove(id);
                    return;

                default:                    
                    return;
            }

            
            ///карантин---------------------------
            //if (update.MyChatMember != null && (update.MyChatMember.OldChatMember.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Kicked
            //    || update.MyChatMember.OldChatMember.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Left))
            //{
            //    try
            //    {
            //        await
            //    }

            //    new Stop().Run(update);
            //    return;
            //}
            ///карантин---------------------------


            if (Users.Contains(id) && Users.At(id).State != User.UserState.Unregistred && Users.At(id).State != User.UserState.None
                && Users.At(id).State != User.UserState.AddGroup && Users.At(id).State != User.UserState.AddSubject
                && Users.At(id).State != User.UserState.Rename)
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    var request = actions[Users.At(id).State].Run(update);

                    await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                        cancellationToken: cancellationToken);

                    if (request.Text != "Назад")
                        await botClient.SendTextMessageAsync(message.Chat, request.Text);
                    if (Users.At(id).State != User.UserState.AddSubject && Users.At(id).State != User.UserState.AddGroup)
                        Users.At(id).State = User.UserState.None;
                }
                else
                {
                    await bot.DeleteMessageAsync(chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        cancellationToken: cancellationToken);
                }
                return;
            }

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {


                if (!Users.Contains(id) && message.Text != "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Вы не зарегистрированы!\n/start для регистрации");
                    return;
                }

                if (Users.Contains(id) && Users.At(id).State != User.UserState.None)
                {
                    var action = actions[Users.At(id).State];
                    await botClient.SendTextMessageAsync(chatId: message.Chat,
                        text: action.Run(update).Text,
                        replyMarkup: action.GetKeyboard(update));
                    return;
                }

                if (commands.ContainsKey(message.Text))
                {
                    var command = commands[message.Text];
                    await botClient.SendTextMessageAsync(chatId: message.Chat,
                        text: command.Run(update).Text,
                        replyMarkup: command.GetKeyboard(update));
                    return;
                }

                await botClient.SendTextMessageAsync(message.Chat, "Введи команду, ящур");
                return;
            }

        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        private async static Task CheckUserStatus(long id)
        {
            try
            {
                await bot.SendTextMessageAsync(id, "оаоаоао таймер");
                //ChatMember chatMember = await bot.GetChatMemberAsync(id, id);
                //bot. 
                //if()
                //{
                //    Console.WriteLine("оаоаоао оно сработало");
                //}
                //chatMember.User.
                //var status = chatMember.Status;
                //if (chatMember.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Kicked )
            }
            catch (ApiRequestException ex)
            {
                if (ex.Message.Contains("Forbidden: bot was blocked by the user"))
                {
                    Groups.Remove(id);
                    Users.Remove(id);
                }
            }
        }

        private static void RemoveInactiveUsers(object obj)
        {
            lock (new object())
            {
                foreach (long id in Users.Keys)
                    CheckUserStatus(id);
            }
        }

        private static void RemoveUsersTimer()
        {
            TimerCallback tm = new TimerCallback(RemoveInactiveUsers);
            Timer timer = new Timer(tm, null, 0, 5000);
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            //TimerCallback tm = new TimerCallback(RemoveInactiveUsers);
            //Timer timer = new Timer(tm, null, 0, 5000);
            //RemoveUsersTimer();

            Console.ReadLine();
        }
    }
}