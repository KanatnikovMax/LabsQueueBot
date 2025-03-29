using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Timers;
using LabsQueueBot.Controller;
using LabsQueueBot.Controller.Commands;
using LabsQueueBot.Controller.Commands.Appliers;
using LabsQueueBot.Controller.Commands.Responders;
using LabsQueueBot.Model;
using LabsQueueBot.Settings;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot
{
    internal static class Program
    {
        private static LabsQueueBotSettings _botSettings;

        /// <summary>
        /// По команде пользователя определяется, как необходимо отреагировать на запрос
        /// </summary>
        private static readonly Dictionary<string, Command> Commands = new()
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
            { "/switch_notification", new SwitchNotification() },
            { "/timetable", new ShowTimetable() },
            { "/show_waiting", new ShowWaiting() }
        };

        /// <summary>
        /// По состоянию пользователя определяется, как необходимо отреагировать на запрос
        /// </summary>
        private static readonly Dictionary<User.UserState, Command> Actions = new()
        {
            { User.UserState.Unregistred, new StartApplier() },
            { User.UserState.UnsetStudentData, new SetGroupApplier() },
            { User.UserState.ChangeData, new SetGroupApplier() },
            { User.UserState.Join, new JoinApplier() },
            { User.UserState.Quit, new QuitApplier() },
            { User.UserState.Skip, new SkipApplier() },
            { User.UserState.ShowQueue, new ShowQueueApplier() },
            { User.UserState.AddSubject, new AddSubjectApplier() },
            { User.UserState.AddGroup, new AddGroupApplier() },
            { User.UserState.Rename, new RenameApplier() },
            { User.UserState.Ban, new BanApplier() },
            { User.UserState.Union, new RandomizeQueueApplier() },
            { User.UserState.SetTimetable, new SetTimetableApplier() },
            { User.UserState.SetTimetableDays, new SetTimetableDaysApplier() },
            { User.UserState.ShowWaiting, new ShowWaitingApplier() }
        };

        private static ITelegramBotClient _bot;
        private static System.Timers.Timer _timer;

        /// <summary>
        /// Обработчик запросов
        /// </summary>
        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            //return;           
            Message message = null;
            try
            {
                //по типу запроса определяется, достоин ли он внимания
                long id;
                switch (update.Type)
                {
                    //случай с клавиатурой
                    case UpdateType.CallbackQuery:
                    {
                        message = update.CallbackQuery.Message;
                        id = message.Chat.Id;

                        //проверяется, что запрос был ответом на вызванный ранее InlineKeyboardMarkup
                        if (Users.Contains(id) && Users.At(id).State == User.UserState.None)
                        {
                            await _bot.DeleteMessageAsync(
                                chatId: message.Chat.Id,
                                messageId: message.MessageId,
                                cancellationToken: cancellationToken);

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                "Введи команду, ящур",
                                cancellationToken: cancellationToken);
                            return;
                        }

                        //проверка регистрации
                        if (!Users.Contains(id))
                        {
                            await _bot.DeleteMessageAsync(
                                chatId: message.Chat.Id,
                                messageId: message.MessageId,
                                cancellationToken: cancellationToken);

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                "Вы не зарегистрированы!\n/start для регистрации",
                                cancellationToken: cancellationToken);
                            return;
                        }

                        break;
                    }
                    //случай с сообщением
                    case UpdateType.Message:
                    {
                        message = update.Message;
                        id = message.Chat.Id;

                        //проверяется, что сообщение действительно является текстовым
                        if (update.Message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
                        {
                            string request;
                            if (Users.Contains(id) && Users.At(id).State != User.UserState.None)
                            {
                                request = "Пришли данные текстом или нажми на кнопку (в зависимости от ситуации)";
                            }
                            else
                            {
                                request = "Не принимаю данные такого типа";
                            }

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: request,
                                cancellationToken: cancellationToken);

                            await botClient.DeleteMessageAsync(
                                chatId: id,
                                messageId: message.MessageId,
                                cancellationToken: cancellationToken);
                            return;
                        }

                        break;
                    }
                    //случай с отпиской от бота
                    case UpdateType.MyChatMember:
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
                                       && !Groups.ContainsKey(
                                           new GroupKey(Users.At(id).CourseNumber, Users.At(id).GroupNumber)))
                {
                    Users.Remove(id);
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: "Вы не зарегистрированы!\n/start для регистрации",
                        cancellationToken: cancellationToken);
                    return;
                }

                //проверяется, что запрос является ответом на вызванный ранее InlineKeyboardMarkup
                if (Users.Contains(id)
                    && Users.At(id).State != User.UserState.Unregistred
                    && Users.At(id).State != User.UserState.None
                    && Users.At(id).State != User.UserState.AddGroup
                    && Users.At(id).State != User.UserState.AddSubject
                    && Users.At(id).State != User.UserState.Rename
                    && Users.At(id).State != User.UserState.SetTimetableDays)
                {
                    //тип запроса - ответ на InlineKeyboardMarkup
                    if (update.Type == UpdateType.CallbackQuery)
                    {
                        try
                        {
                            //вызов соответствующего ответа на запрос
                            var request = Actions[Users.At(id).State].Run(update);

                            //удаление InlineKeyboardMarkup
                            await _bot.DeleteMessageAsync(
                                chatId: message.Chat.Id,
                                messageId: message.MessageId,
                                cancellationToken: cancellationToken);

                            if (request.Text != "Назад")
                                await botClient.SendTextMessageAsync(
                                    chatId: message.Chat,
                                    text: request.Text,
                                    cancellationToken: cancellationToken);

                            if (Users.At(id).State == User.UserState.Union)
                            {
                                Users.At(id).State = User.UserState.None;
                                NotifyGroup(id);
                            }

                            if (Users.At(id).State != User.UserState.AddSubject
                                && Users.At(id).State != User.UserState.AddGroup
                                && Users.At(id).State != User.UserState.SetTimetableDays)
                            {
                                Users.At(id).State = User.UserState.None;
                            }
                        }
                        catch (InvalidOperationException) //если запрос был ответом на неактуальный InlineKeyboardMarkup
                        {
                            await _bot.DeleteMessageAsync(
                                chatId: message.Chat.Id,
                                messageId: message.MessageId,
                                cancellationToken: cancellationToken);

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: "Нажми на нужную табличку",
                                cancellationToken: cancellationToken);
                        }
                    }
                    else //иначе удаление запроса пользователя
                    {
                        await _bot.DeleteMessageAsync(
                            chatId: message.Chat.Id,
                            messageId: message.MessageId,
                            cancellationToken: cancellationToken);
                    }

                    return;
                }

                if (update.Type == UpdateType.Message) //тип запроса - текстовое сообщение
                {
                    //проверка регистрации
                    if (!Users.Contains(id) && message.Text != "/start")
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: "Вы не зарегистрированы!\n/start для регистрации",
                            cancellationToken: cancellationToken);
                        return;
                    }

                    //вызов соответствующего ответа на существующий запрос
                    if (Users.Contains(id) && Users.At(id).State != User.UserState.None)
                    {
                        var action = Actions[Users.At(id).State];
                        await botClient.SendTextMessageAsync(chatId: message.Chat,
                            text: action.Run(update).Text,
                            replyMarkup: action.GetKeyboard(update),
                            cancellationToken: cancellationToken);
                        return;
                    }

                    //для обработки команды бана
                    var splitBanMessage = message.Text.Split('\n');
                    var isBanCommand = splitBanMessage.Length == 2
                                       && Commands.ContainsKey(splitBanMessage[0])
                                       && Commands[splitBanMessage[0]].Definition.StartsWith("/ban_person");

                    //вызов соответствующего ответа на запрос с командой
                    if (Commands.ContainsKey(message.Text) || isBanCommand)
                    {
                        var command = isBanCommand
                            ? Commands[splitBanMessage[0]]
                            : Commands[message.Text];

                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: command.Run(update).Text,
                            replyMarkup: command.GetKeyboard(update),
                            cancellationToken: cancellationToken);

                        return;
                    }

                    //запрос не являлся валидным
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: "Введи команду, ящур",
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();
                var lastUpdates = await _bot.GetUpdatesAsync(
                    limit: 10,
                    cancellationToken: cancellationToken);
                sb.AppendLine(e.Message);
                sb.AppendLine("---");
                sb.AppendLine(e.StackTrace);
                sb.AppendLine("---");
                sb.AppendLine(JsonConvert.SerializeObject(update));
                sb.AppendLine("---");
                sb.AppendLine(JsonConvert.SerializeObject(lastUpdates));
                const string path = "ErrorReason.txt";
                await System.IO.File.WriteAllTextAsync(path, sb.ToString(), cancellationToken);
                foreach (var logChatTgId in _botSettings.LogChatTgIds)
                {
                    await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    var doc = new InputFileStream(stream, path);
                    await botClient.SendDocumentAsync(
                        chatId: logChatTgId,
                        document: doc,
                        caption: e is DbUpdateConcurrencyException or DbUpdateException
                            ? "Database Error"
                            : "User's Request Error",
                        cancellationToken: cancellationToken);
                }

                if (message is not null)
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Ошибка, попробуйте ещё раз позже",
                        cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Обработчик исключений
        /// </summary>
        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(JsonConvert.SerializeObject(exception));
            var lastUpdates = await _bot.GetUpdatesAsync(
                offset: 10,
                limit: 10,
                cancellationToken: cancellationToken);
            foreach (var update in lastUpdates
                         .Where(update =>
                             update.Type == UpdateType.CallbackQuery))
            {
                Console.WriteLine(JsonConvert.SerializeObject(update));
            }
        }

        /// <summary>
        /// Инициирует рассылку для пользователя
        /// </summary>
        /// <param name="id"> Id пользователя </param>
        private static async void MassSender(long id)
        {
            var builder = new StringBuilder();
            builder.AppendLine(Groups.ShowSubjects(id));
            builder.AppendLine(new SwitchNotification().Definition);
            await _bot.SendTextMessageAsync(
                chatId: id,
                text: builder.ToString());
        }

        /// <summary>
        /// Запускает массовую рассылку об изменениях для всех пользователей
        /// </summary>
        private static void Send(object? s, ElapsedEventArgs e)
        {
            foreach (var id in Users.Keys
                         .Where(x => Users.At(x).State == User.UserState.None
                                     && Users.At(x).IsNotifyNeeded))
            {
                MassSender(id);
            }
        }

        /// <summary>
        /// Формирует очереди в группах по предметам по заданным дням недели
        /// </summary>
        private static void UnionByDayOfWeek(object? s, ElapsedEventArgs e)
        {
            foreach (var group in Groups.groups)
            {
                foreach (var subject in group.Value.Timetable
                             .Where(subject =>
                                 group.Value.ContainsKey(subject.Key)
                                 && subject.Value.Contains(DateTime.UtcNow.DayOfWeek)))
                {
                    group.Value[subject.Key].Union();
                }
            }
        }

        /// <summary>
        /// Запускает массовую рассылку об изменениях для группы вызвавшего рассылку
        /// </summary>
        /// <param name="id"></param>
        private static void NotifyGroup(long id)
        {
            var currentUser = Users.At(id);

            foreach (var user in Users.Values
                         .Where(user => user.State == User.UserState.None
                                        && currentUser.CourseNumber == user.CourseNumber
                                        && currentUser.GroupNumber == user.GroupNumber
                                        && currentUser.IsNotifyNeeded))
            {
                MassSender(user.Id);
            }
        }

        /// <summary>
        /// Запускает таймер, который в заданное время инициирует формирование очередей
        /// и массовую отправку уведомлений об изменениях
        /// </summary>
        private static Task StartTimer()
        {
            var interval = CalculateInterval(DateTime.UtcNow);
            _timer = new System.Timers.Timer(interval);
            _timer.Elapsed += UnionByDayOfWeek;
            _timer.Elapsed += Send;
            _timer.Elapsed += (_, _) => _timer.Interval = CalculateInterval(DateTime.UtcNow);
            _timer.AutoReset = true;
            _timer.Enabled = true;

            return Task.CompletedTask;

            double CalculateInterval(DateTime dateTimeNow)
            {
                var notificationTime = DateTime.ParseExact(
                    _botSettings.TimeForNotification,
                    "HH-mm-ss",
                    CultureInfo.InvariantCulture).AddHours(-3);

                var nextRun = dateTimeNow.Date
                    .AddHours(notificationTime.Hour)
                    .AddMinutes(notificationTime.Minute)
                    .AddSeconds(notificationTime.Second);
                if (nextRun <= dateTimeNow)
                {
                    nextRun = nextRun.AddDays(1);
                }

                return (nextRun - dateTimeNow).TotalMilliseconds;
            }
        }

        private static async Task Main()
        {
            //Славянский ретёрн в мэйне
            //return;

            //конфигурация переменных окружения
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettingsDevActive.json", optional: true, reloadOnChange: true)
                .Build();
            _botSettings = LabsQueueBotSettingsReader.Read(configuration);

            _bot = new TelegramBotClient(_botSettings.BotToken);

            //генерация пароля
            PasswordGenerator.Generate(10);
            Commands.Add($"/randomize_queue {PasswordGenerator.Password}", new RandomizeQueue());
            Commands.Add($"/ban_person {PasswordGenerator.Password}", new BanUserFromQueue());
            Commands.Add($"/set_timetable {PasswordGenerator.Password}", new SetTimetable());

            Console.WriteLine("Запущен бот " + _bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };

            //запуск бота            
            _bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

            Console.WriteLine($"Admin password: {PasswordGenerator.Password}");
            foreach (var adminChatTgId in _botSettings.AdminChatTgIds)
            {
                try
                {
                    await _bot.SendTextMessageAsync(
                        chatId: adminChatTgId,
                        text: $"Password: {PasswordGenerator.Password}",
                        cancellationToken: cancellationToken);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            //запуск таймера для рассылки
            await StartTimer();
            await Task.Delay(-1);
        }
    }
}