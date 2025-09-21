using System.Text;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class HelpCommandExecutor(
    Func<IEnumerable<ICommandExecutor>> commands,
    QueueBotSettings botSettings,
    CommandsSettings commandsSettings) : ICommandExecutor
{
    public string Type => commandsSettings.HelpCommand.Type;
    public string Name => commandsSettings.HelpCommand.Name;
    public IReadOnlyCollection<UserState> States => [];
    public Role AcceptRole => Role.Nobody;
    public string Definition => commandsSettings.HelpCommand.Definition;
    public async Task Execute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var commandsDescription = GetDescription(user.Role);
        
        var builder = new StringBuilder();
        builder.AppendLine("При добавлении в очередь пользователь записывается в список ожидающих. "
                           + $"В установленный день в {botSettings.UnionNotificationTimeUtc} список ожидающих случайным образом перемешивается и добавляется в конец "
                           + "соответствующей очереди, тем, кто подписан на рассылку, приходит уведомление с его местами в очередях, "
                           + "в которые он записан. Пользователи с админскими правами соответствующей командой "
                           + "могут вызвать генерацию очередей для своей группы в любой момент времени и изменять расписание для генерации. "
                           + "Для получения админских прав староста группы (или другое ответственное лицо) должен написать админу "
                           + "бота в лс (ссылка на профиль админа в описании)");
        
        builder.AppendLine();
        builder.AppendLine(commandsDescription);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: builder.ToString(),
            cancellationToken: cancellationToken);
    }
    
    private string GetDescription(Role role)
    {
        var builder = new StringBuilder();
        
        var commandTypes = commands.Invoke()
            .Where(c => c.AcceptRole <= role && !string.IsNullOrEmpty(c.Type))
            .Select(c => c.Type)
            .Distinct();
        foreach (var type in commandTypes)
        {
            if (type is null)
                continue;
            
            var descriptions = commands.Invoke()
                .Where(c => c.AcceptRole <= role && type == c.Type)
                .Select(c => $"{c.Name} - {c.Definition}")
                .ToList();
            if (descriptions.Count == 0)
                continue;
            
            builder.AppendLine(type);
            foreach (var description in descriptions)
            {
                builder.AppendLine(description);
            }
        }

        return builder.ToString();
    }
}