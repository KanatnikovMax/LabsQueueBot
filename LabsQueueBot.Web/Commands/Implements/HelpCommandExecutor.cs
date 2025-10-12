using System.Text;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class HelpCommandExecutor(
    Func<IEnumerable<ICommandExecutor>> commands,
    IOptions<TelegramBotSettings> botOptions,
    IOptions<UnionJobSettings> jobOptions,
    IOptions<CommandsSettings> commandsOptions,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string InformationMessage =
        """
        При добавлении в очередь пользователь записывается в список ожидающих.{0}
        Пользователи с правами администратора соответствующей командой могут вызвать формирование очередей для своей группы в любой момент времени.
        Администратор может изменять расписание автоматического формирования очередей для отдельных дисциплин, если оно включено.
        Для получения прав администратора староста группы (или другой ответственный гражданин) должен написать админу бота в лс (ссылка на профиль админа в описании).
        Пользователи, которые подписаны на рассылку, получают уведомления о своих местах в очередях после их формирования.
        
        """;

    private const string NotificationMessagePart =
        """
        
        Для каждой дисциплины в установленный день в {0} список ожидающих случайным образом перемешивается и добавляется в конец соответствующей очереди.
        """;
    
    public override string Type => commandsOptions.Value.Help.Type;
    public override string Name => commandsOptions.Value.Help.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [];
    public override Role AcceptRole => Role.Nobody;
    public override string Definition => commandsOptions.Value.Help.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var notificationInfo = string.Empty;
        if (jobOptions.Value.IsEnabled)
        {
            var notificationTime = jobOptions.Value.UnionTimeUtc + TimeSpan.FromHours(botOptions.Value.LocalUtcOffset);
            notificationInfo = string.Format(NotificationMessagePart, notificationTime.ToString("hh\\:mm"));
        }
        
        var commandsDescription = GetDescription(user.Role);
        
        var builder = new StringBuilder();
        
        builder.AppendLine(string.Format(InformationMessage, notificationInfo));
        builder.AppendLine(commandsDescription);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: builder.ToString(),
            cancellationToken: cancellationToken);

        return true;
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
            
            builder.AppendLine().AppendLine(type);
            foreach (var description in descriptions)
            {
                builder.AppendLine(description);
            }
        }

        return builder.ToString();
    }
}