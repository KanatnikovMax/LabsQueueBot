using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace LabsQueueBot.Web.Controller;

[ApiController]
[Route("/amm_labs_queue_bot")]
public class LabsQueueBotController(ITelegramBotClient botClient, IUpdateHandler updateHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update, CancellationToken cancellationToken)
    {
        await updateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
        return Ok();
    }
}