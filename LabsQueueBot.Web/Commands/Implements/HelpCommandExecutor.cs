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
    IOptions<CommandsSettings> commandsOptions,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string InformationMessage = """
                                              При добавлении в очередь пользователь записывается в список ожидающих.
                                              В установленный день в {0} список ожидающих случайным образом перемешивается и добавляется в конец соответствующей очереди.
                                              Тем, кто подписан на рассылку, приходит уведомление с его местами в очередях, в которые он записан.
                                              Пользователи с правами администратора соответствующей командой могут вызвать генерацию очередей для своей группы в любой момент времени и изменять расписание генерации.
                                              Для получения прав администратора староста группы (или другое ответственный гражданин) должен написать админу бота в лс (ссылка на профиль админа в описании).
                                              
                                              """;
    
    public override string Type => commandsOptions.Value.Help.Type;
    public override string Name => commandsOptions.Value.Help.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [];
    public override Role AcceptRole => Role.Nobody;
    public override string Definition => commandsOptions.Value.Help.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var notificationTime = botOptions.Value.UnionTimeUtc + TimeSpan.FromHours(botOptions.Value.LocalUtcOffset);
        
        var commandsDescription = GetDescription(user.Role);
        
        var builder = new StringBuilder();
        
        builder.AppendLine(string.Format(InformationMessage, notificationTime.ToString("hh\\:mm")));
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
            
            builder.AppendLine(type);
            foreach (var description in descriptions)
            {
                builder.AppendLine(description);
            }
        }

        return builder.ToString();
    }
}