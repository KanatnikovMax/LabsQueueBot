using LabsQueueBot.Core.Settings;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.BackgroundServices;

public class LabsQueueBotService(
    ILogger logger,
    ITelegramBotClient tgBotClient,
    IUpdateHandler updateHandler,
    QueueBotSettings settings)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.Information("LabsQueueBot Initialized");
        
        tgBotClient.StartReceiving(
            updateHandler.HandleUpdateAsync,
            updateHandler.HandlePollingErrorAsync,
            new ReceiverOptions
            {
                AllowedUpdates = [
                    UpdateType.Message,
                    UpdateType.CallbackQuery, 
                    UpdateType.MyChatMember
                ],
            },
            cancellationToken
        );
        
        logger.Information("LabsQueueBot started");

        foreach (var id in settings.AdminChatId)
        {
            await tgBotClient.SendTextMessageAsync(
                chatId: id,
                text: "hello world!",
                cancellationToken: cancellationToken);
        }
        
        await Task.Delay(-1, cancellationToken);
    }
}