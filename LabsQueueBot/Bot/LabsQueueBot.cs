using LabsQueueBot.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace LabsQueueBot.Bot;

public class LabsQueueBot(ILogger logger, IUpdateHandler handler, LabsQueueBotSettings settings)
    : BackgroundService, ITelegramBot
{
    private ILogger _logger = logger;
    private readonly ITelegramBotClient _botClient = new TelegramBotClient(settings.BotToken);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _botClient.StartReceiving(
            handler.HandleUpdateAsync,
            handler.HandlePollingErrorAsync,
            new ReceiverOptions
            {
                AllowedUpdates = []
            },
            stoppingToken
        );

        Task.Delay(-1, stoppingToken);
        return Task.CompletedTask;
    }
}