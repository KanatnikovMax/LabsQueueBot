using LabsQueueBot.Core.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Jobs;

public class LabsQueueBotStartingJob(
    ILogger logger,
    ITelegramBotClient tgBotClient,
    IUpdateHandler updateHandler,
    IOptions<TelegramBotSettings> options)
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

        foreach (var id in options.Value.AdminChatId)
        {
            await tgBotClient.SendTextMessageAsync(
                chatId: id,
                text: "hello world!",
                cancellationToken: cancellationToken);
        }
        
        await Task.Delay(-1, cancellationToken);
    }
}