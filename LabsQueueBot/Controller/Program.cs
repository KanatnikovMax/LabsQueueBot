﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using System.Timers;

namespace LabsQueueBot
{
    class Program
    {
        /// <summary>
        /// По команде пользователя определяется, как необходимо отреагировать на запрос
        /// </summary>
        public static readonly Dictionary<string, Command> commands = new()
        {
            { "/start", new Start() },
            { "/stop", new Stop() },
            { "/help", new Help() },
            { "/join", new Join() },
            { "/quit", new Quit() },
            { "/skip", new Skip() },
            { "/change_group", new SetGroup() },
            { "/subjects", new Subjects() },
            { "/show", new Show() },
            { "/rename", new Rename() },
            { "/switch_notification", new SwitchNotification() }
        };

        /// <summary>
        /// По состоянию пользователя определяется, как необходимо отреагировать на запрос
        /// </summary>
        public static readonly Dictionary<User.UserState, Command> actions = new()
        {
            { User.UserState.Unregistred, new StartApplier() },
            { User.UserState.UnsetStudentData, new SetGroupApplier() },
            { User.UserState.ChangeData, new SetGroupApplier() },
            { User.UserState.Join, new JoinApplier() },
            { User.UserState.Quit, new QuitApplier() },
            { User.UserState.Skip, new SkipApplier() },
            { User.UserState.ShowQueue, new ShowQueueApplier() },
            { User.UserState.AddSubject, new AddSubjectApplier() }, //
            { User.UserState.AddGroup, new AddGroupApplier() }, //
            { User.UserState.Rename, new RenameApplier() } //
        };

        private static ITelegramBotClient bot = new TelegramBotClient("7098667146:AAHlUf4Y-cmOtkOmCcvFDVnKFHbkVlCgpJE");
        private static System.Timers.Timer _timer;

        /// <summary>
        /// Обработчик запросов
        /// </summary>
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            long id = 0;
            Message message;

            //по типу запроса определяется, достоин ли он внимания
            switch (update.Type)
            {
                //случай с клавиатурой
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                {
                    message = update.CallbackQuery.Message;
                    id = message.Chat.Id;

                    //проверяется, что запрос был ответом на вызванный ранее InlineKeyboardMarkup
                    if (Users.Contains(id) && Users.At(id).State == User.UserState.None)
                    {
                        await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                            cancellationToken: cancellationToken);
                        await botClient.SendTextMessageAsync(message.Chat, "Введи команду, ящур");
                        return;
                    }

                    //проверка регистрации
                    if (!Users.Contains(id))
                    {
                        await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                            cancellationToken: cancellationToken);
                        await botClient.SendTextMessageAsync(message.Chat,
                            "Вы не зарегистрированы!\n/start для регистрации");
                        return;
                    }

                    break;
                }
                //случай с сообщением
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                {
                    message = update.Message;
                    id = message.Chat.Id;
                    //проверяется, что сообщение действительно является текстовым
                    if (update.Message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
                    {
                        string request;
                        if (Users.Contains(id) && Users.At(id).State != User.UserState.None)
                            request = "Пришли данные текстом или нажми на кнопку (в зависимости от ситуации)";
                        else
                            request = "Не принимаю данные такого типа";
                        await botClient.SendTextMessageAsync(message.Chat, request);
                        await botClient.DeleteMessageAsync(chatId: id, messageId: message.MessageId);
                        return;
                    }

                    break;
                }
                //случай с отпиской от бота
                case Telegram.Bot.Types.Enums.UpdateType.MyChatMember:
                {
                    id = update.MyChatMember.Chat.Id;
                    message = null;
                    if (Users.Contains(id))
                        Groups.Remove(id);
                    Users.Remove(id);
                    return;
                }
                default:
                    return;
            }

            //проверяется регистрация пользователя
            if (Users.Contains(id) && Users.At(id).State == User.UserState.None
                                   && !Groups.ContainsKey(new GroupKey(Users.At(id).CourseNumber,
                                       Users.At(id).GroupNumber)))
            {
                Users.Remove(id);
                await botClient.SendTextMessageAsync(message.Chat, "Вы не зарегистрированы!\n/start для регистрации");
                return;
            }

            //проверяется, что запрос является ответом на вызванный ранее InlineKeyboardMarkup
            if (Users.Contains(id) && Users.At(id).State != User.UserState.Unregistred &&
                Users.At(id).State != User.UserState.None
                && Users.At(id).State != User.UserState.AddGroup && Users.At(id).State != User.UserState.AddSubject
                && Users.At(id).State != User.UserState.Rename)
            {
                //тип запроса - ответ на InlineKeyboardMarkup
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    try
                    {
                        //вызов соответствующего ответа на запрос
                        var request = actions[Users.At(id).State].Run(update);

                        //удаление InlineKeyboardMarkup
                        await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                            cancellationToken: cancellationToken);

                        if (request.Text != "Назад")
                            await botClient.SendTextMessageAsync(message.Chat, request.Text);

                        if (Users.At(id).State != User.UserState.AddSubject
                            && Users.At(id).State != User.UserState.AddGroup)
                        {
                            Users.At(id).State = User.UserState.None;
                        }
                    }
                    //если запрос был ответом на неактуальный InlineKeyboardMarkup
                    catch (InvalidOperationException)
                    {
                        await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                            cancellationToken: cancellationToken);
                        await botClient.SendTextMessageAsync(message.Chat, "Нажми на нужную табличку");
                    }
                }
                //иначе удаление запроса пользователя
                else
                {
                    await bot.DeleteMessageAsync(chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        cancellationToken: cancellationToken);
                }

                return;
            }

            //тип запроса - текстовое сообщение
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                //проверка регистрации
                if (!Users.Contains(id) && message.Text != "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat,
                        "Вы не зарегистрированы!\n/start для регистрации");
                    return;
                }

                //вызов соответствующего ответа на существующий запрос
                if (Users.Contains(id) && Users.At(id).State != User.UserState.None)
                {
                    var action = actions[Users.At(id).State];
                    await botClient.SendTextMessageAsync(chatId: message.Chat,
                        text: action.Run(update).Text,
                        replyMarkup: action.GetKeyboard(update));
                    return;
                }

                //вызов соответствующего ответа на запрос с командой
                if (commands.ContainsKey(message.Text))
                {
                    var command = commands[message.Text];
                    await botClient.SendTextMessageAsync(chatId: message.Chat,
                        text: command.Run(update).Text,
                        replyMarkup: command.GetKeyboard(update));
                    return;
                }

                //запрос не являлся валидным
                await botClient.SendTextMessageAsync(message.Chat, "Введи команду, ящур");
            }
        }


        /// <summary>
        /// Обработчик исключений
        /// </summary>
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            List<Update> lastUpdates = bot.GetUpdatesAsync(10, 10, null, null, cancellationToken).Result.ToList();
            foreach (var update in lastUpdates)
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
        }

        /// <summary>
        /// Инициирует рассылку для пользователя
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        private static async void MassSendler(long id)
        {
            await bot.SendTextMessageAsync(id, Groups.ShowSubjects(id));
        }

        /// <summary>
        /// Объединяет очереди и списки ожидания, запускает массовую рассылку об изменениях
        /// </summary>
        private static void UnionAndSend(object s, ElapsedEventArgs e)
        {
            Groups.Union();
            foreach (var id in Users.Keys
                         .Where(x => Users.At(x).State == User.UserState.None
                                     && Users.At(x).IsNotifyNeeded))
            {
                MassSendler(id);
            }
        }

        /// <summary>
        /// Запускает таймер, который инициирует объединение очередей и списков ожидания, а после - массовую отправку уведомлений об изменениях
        /// </summary>
        private static Task StartTimer()
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(19);
            if (nextRun <= now)
            {
                nextRun = nextRun.AddDays(1);
            }

            double interval = (nextRun - now).TotalMilliseconds;


            _timer = new System.Timers.Timer(interval);
            _timer.Elapsed += UnionAndSend;
            _timer.Elapsed += (_, _) => _timer.Interval = TimeSpan.FromDays(1).TotalMilliseconds;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            return Task.CompletedTask;
        }

        static async Task Main(string[] args)
        {
            //return;

            //генерация пароля
            PasswordGenerator.Generate(10);
            commands.Add($"/randomize_queue {PasswordGenerator.Password}", new RandomizeQueue());
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            //запуск бота
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
            await bot.SendTextMessageAsync(5083997588, PasswordGenerator.Password);

            //запуск таймера для рассылки
            await StartTimer();
            await Task.Delay(-1);
        }
    }
}