using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.Providers;
using LabsQueueBot.Web.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class SwitchWeekNumerationCommandExecutor(
    IWeekProvider weekProvider,
    IAdminNotificationService notificationService,
    IOptions<CommandsSettings> commandsOptions,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string SuccessMessage = "Текущая неделя изменена на {0}";
    
    public override string Type => commandsOptions.Value.SwitchWeekNumeration.Type;
    public override string Name => commandsOptions.Value.SwitchWeekNumeration.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.None, UpdateType.Message) ];
    public override Role AcceptRole => Role.Admin;
    public override string Definition => commandsOptions.Value.SwitchWeekNumeration.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        weekProvider.SwitchWeekNumerate();
        
        var weekName = weekProvider.GetCurrentWeekName();
        var message = string.Format(SuccessMessage, weekName);
        await notificationService.NotifyWithMessage(message, cancellationToken);
        
        return true;
    }
}