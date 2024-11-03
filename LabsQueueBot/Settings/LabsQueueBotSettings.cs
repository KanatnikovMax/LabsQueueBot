namespace LabsQueueBot.Settings;

public class LabsQueueBotSettings
{
    public string BotToken { get; set; }
    public List<long> AdminChatTgIds { get; set; }
    public List<long> LogChatTgIds { get; set; }
    public string TimeForNotification { get; set; }
}