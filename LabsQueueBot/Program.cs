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


namespace LabsQueueBot
{
    

    class Program
    {
        internal static readonly Dictionary<string, Command> commands = new()
        {
            {"/start", new Start() },
            {"/stop", new Stop() },
            {"/join", new Join() },
            {"/quit", new Quit() },
            {"/change_info", new ChangeGroup() },
            {"/help", new Help() },
            {"/skip", new Skip() },
            {"/subjects", new Subjects() },
            {"/show", null }
        };

        internal static readonly Dictionary<User.UserState, Command> actions = new()
        {
            {User.UserState.UnsetStudentData, new StartApplier() },
            {User.UserState.SetGroup, null },
            {User.UserState.ChoiceSubject, null },
            {User.UserState.Validation, null }
        };

        static ITelegramBotClient bot = new TelegramBotClient("...");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                long id = update.Message.Chat.Id;

                //TODO: обновить все имеющиеся данные по очередям во время потоки.sleep

                if (!Users.Contains(id) && message.Text != "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Вы не зарегистрированы!\n/start для регистрации");
                    return;
                }

                if (Users.Contains(id) && Users.At(id).State != User.UserState.None)
                {
                    SendMessageRequest request = actions[Users.At(id).State].Run(update);
                    await botClient.SendTextMessageAsync(message.Chat, request.Text);
                    return;
                }
               
                if (commands.ContainsKey(message.Text))
                {
                    SendMessageRequest request = commands[message.Text].Run(update);
                    await botClient.SendTextMessageAsync(message.Chat, request.Text);
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat, "Введи команду, ящур");
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
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
            Console.ReadLine();
        }
    }
}