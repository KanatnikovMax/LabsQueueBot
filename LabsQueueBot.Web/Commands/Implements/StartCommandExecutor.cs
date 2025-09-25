using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Validators;
using LabsQueueBot.Repository.Repository;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Commands.Implements;

public class StartCommandExecutor(
    IUserRepository usersRepository,
    ILogger logger,
    CommandsSettings commandsSettings) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string AlreadyRegisteredMessage = "Ты уже зареган\nИди отсюда, розбийник";
    private const string WhoAreYouMessage = "Кто ты, воин?\n\nВведи свои данные в формате\nФамилия Имя";
    private const string InvalidUserNameMessage = "{0}\nПовторите ввод";
    private const string SuccessMessage = "Регистрация успешно завершена!\nОсталось выбрать курс и группу =)\n{0}";
    
    public override string Type { get; } = commandsSettings.StartCommand.Type;
    public override string Name { get; } = commandsSettings.StartCommand.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ 
        (UserState.Unregistered, UpdateType.Message),
        (UserState.Register, UpdateType.Message)
    ];
    public override Role AcceptRole => Role.Nobody;
    public override string Definition { get; } = commandsSettings.StartCommand.Definition;

    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        switch (user.State)
        {
            case UserState.Unregistered:
            {
                await StartRegistration(botClient, user, cancellationToken);
                break;
            }
            case UserState.Register:
            {
                await CompleteRegistration(botClient, update, user, cancellationToken);
                break;
            }
            default:
            {
                await AlreadyRegistered(botClient, user, cancellationToken);
                break;
            }
        }
        
        return true;
    }

    private async Task StartRegistration(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.Register;
        user = await usersRepository.SaveAsync(user, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: WhoAreYouMessage,
            cancellationToken: cancellationToken);
    }

    private async Task CompleteRegistration(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var validationResult = UserInfoValidator.ValidateName(update.Message!.Text);
        if (validationResult != null)
        {
            user.State = UserState.Register;
            await usersRepository.SaveAsync(user, cancellationToken);
                
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: string.Format(InvalidUserNameMessage, validationResult),
                cancellationToken: cancellationToken);
            return;
        }
        
        user.Name = update.Message!.Text!;
        user.State = UserState.None;
        await usersRepository.SaveAsync(user, cancellationToken);
        
        await botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: string.Format(SuccessMessage, commandsSettings.SetGroupCommand.Name),
            cancellationToken: cancellationToken); 
    }

    private async Task AlreadyRegistered(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
        => await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: AlreadyRegisteredMessage,
            cancellationToken: cancellationToken);
}