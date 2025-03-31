using LabsQueueBot.Commands;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.Db.Entities.User;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Bot;

public class UpdateHandler(ILogger logger, CommandFactory commandFactory, IRepository<User> usersRepository)
    : IUpdateHandler
{
    private readonly ILogger _logger = logger;

    private const string WrongCommandRequestMessage = "Введи команду, ящур";

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(update.Message.Chat.Id, cancellationToken);
        if (user is null)
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Вы не зарегистрированы!\n/start для регистрации",
                cancellationToken: cancellationToken);
            return;
        }

        if (update.Type != UpdateType.Message
            && update.Type == UpdateType.Message && update.Message.Type != MessageType.Text
            && update.Type != UpdateType.CallbackQuery
            && update.Type != UpdateType.MyChatMember)
        {
            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.MessageId,
                cancellationToken: cancellationToken);

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: WrongCommandRequestMessage,
                cancellationToken: cancellationToken);
            return;
        }

        try
        {
            if (update.Type == UpdateType.MyChatMember)
            {
                await HandleMyChatMember(update, cancellationToken);
            }
            else
            {
                await HandleDefaultUpdate(botClient, update, cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.Warning(e.Message);
        }
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        logger.Fatal(JsonConvert.SerializeObject(exception));
        var lastUpdates = await botClient.GetUpdatesAsync(
            offset: 10,
            limit: 10,
            cancellationToken: cancellationToken);
        foreach (var update in lastUpdates
                     .Where(update =>
                         update.Type == UpdateType.CallbackQuery))
        {
            logger.Fatal(JsonConvert.SerializeObject(update));
        }
    }
    
    private async Task HandleDefaultUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var id = update.Message.Chat.Id;
        var user = await usersRepository.GetByIdAsync(id, cancellationToken);

        var command = user.State == User.UserState.None
            ? commandFactory.GetCommand(update.Message.Text)
            : commandFactory.GetCommand(user.State);

        if (command is not null)
        {
            await command.Execute(botClient, update, cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: WrongCommandRequestMessage,
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleMyChatMember(Update update, CancellationToken cancellationToken)
    {
        var id = update.MyChatMember.Chat.Id;
        var user = await usersRepository.GetByIdAsync(id, cancellationToken);
        await usersRepository.DeleteAsync(user, cancellationToken);
    }
}