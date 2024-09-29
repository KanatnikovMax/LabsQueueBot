using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;

namespace LabsQueueBot
{
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
            {"/switch_notification", new SwitchNotification() }
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
            {User.UserState.AddSubject, new AddSubjectApplier() }, //
            {User.UserState.AddGroup, new AddGroupApplier() }, //
            {User.UserState.Rename, new RenameApplier() } //
        };

        static ITelegramBotClient bot = new TelegramBotClient("7098667146:AAHlUf4Y-cmOtkOmCcvFDVnKFHbkVlCgpJE");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //using (var sw = new StreamWriter("file.txt", true, Encoding.UTF8)) 
            //{
            //    sw.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            //}
            //return;
            long id = 0;
            Message message;
            
            
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:

                    message = update.CallbackQuery.Message;
                    id = message.Chat.Id;

                    if (Users.Contains(id) && Users.At(id).State == User.UserState.None)
                    {
                        await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                            cancellationToken: cancellationToken);
                        await botClient.SendTextMessageAsync(message.Chat, "Введи команду, ящур");
                        return;
                    }

                    if (!Users.Contains(id))
                    {
                        await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                            cancellationToken: cancellationToken);
                        await botClient.SendTextMessageAsync(message.Chat, "Вы не зарегистрированы!\n/start для регистрации");
                        return;
                    }
                    break;

                case Telegram.Bot.Types.Enums.UpdateType.Message:

                    message = update.Message;
                    id = message.Chat.Id;
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

            if (Users.Contains(id) && Users.At(id).State == User.UserState.None
                && !Groups.ContainsKey(new GroupKey(Users.At(id).CourseNumber, Users.At(id).GroupNumber)))
            {
                Users.Remove(id);
                await botClient.SendTextMessageAsync(message.Chat, "Вы не зарегистрированы!\n/start для регистрации");
                return;
            }

            if (Users.Contains(id) && Users.At(id).State != User.UserState.Unregistred && Users.At(id).State != User.UserState.None
                && Users.At(id).State != User.UserState.AddGroup && Users.At(id).State != User.UserState.AddSubject
                && Users.At(id).State != User.UserState.Rename)
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    try
                    {
                        var request = actions[Users.At(id).State].Run(update);

                        await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                            cancellationToken: cancellationToken);

                        if (request.Text != "Назад")
                            await botClient.SendTextMessageAsync(message.Chat, request.Text);

                        if (Users.At(id).State != User.UserState.AddSubject && Users.At(id).State != User.UserState.AddGroup)
                            Users.At(id).State = User.UserState.None;
                    }
                    catch (InvalidOperationException)
                    {
                        await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                            cancellationToken: cancellationToken);
                        await botClient.SendTextMessageAsync(message.Chat, "Нажми на нужную табличку");
                    }
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
            
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            List<Update> lastUpdates = bot.GetUpdatesAsync(10, 10, null, null, cancellationToken).Result.ToList();
            foreach (var update in lastUpdates)
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
        }        

        public static async void MassSendler(long id)
        {
            await bot.SendTextMessageAsync(id, Groups.ShowSubjects(id));
        }
        static async Task Main(string[] args)
        {
            //return;
            
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            //await Task.Delay(-1);
            while (true)
            {
                Console.ReadLine();
                Groups.Union();
                foreach (var id in Users.Keys
                    .Where(x => Users.At(x).State == User.UserState.None
                    && Users.At(x).IsNotifyNeeded))
                    MassSendler(id);
            }
        }
    }
}