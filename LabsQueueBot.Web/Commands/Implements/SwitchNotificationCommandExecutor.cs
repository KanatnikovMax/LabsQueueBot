using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Repository.Repository;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class SwitchNotificationCommandExecutor(
    IUserRepository userRepository,
    IOptions<CommandsSettings> options,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string NotifyOnMessage = "Вы подписались на массовую рассылку";
    private const string NotifyOffMessage = "Вы отписались от массовой рассылки";
    
    public override string Type => options.Value.SwitchNotification.Type;
    public override string Name => options.Value.SwitchNotification.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [];
    public override Role AcceptRole => Role.Default;
    public override string Definition => options.Value.SwitchNotification.Definition;

    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        user.IsNotifyNeeded = !user.IsNotifyNeeded;
        await userRepository.SaveAsync(user, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: user.IsNotifyNeeded
                ? NotifyOnMessage
                : NotifyOffMessage,
            cancellationToken: cancellationToken);

        return true;
    }
}