using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LabsQueueBot.Commands.Implements;

public class ShowCommand : ICommand
{
    public string Name => "/show";
    
    public string Definition => "";

    public ShowCommand(ILogger _logger)
    {
        
    }
    
    public Task Execute(ITelegramBotClient botClient, Update update, CancellationToken stoppingToken)
    {
        
        
        
        throw new NotImplementedException();
    }

    public bool Contains(string command)
    {
        return Name.Equals(command);
    }
}