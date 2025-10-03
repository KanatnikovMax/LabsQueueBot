using System.Text.RegularExpressions;
using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Validators;
using LabsQueueBot.Repository.Repository;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class UnbanCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectRepository,
    IBlackListManagementService blackListManagementService,
    IOptions<TelegramBotSettings> botOptions,
    IOptions<CommandsSettings> commandsOptions,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string CallAdminMessage =
        """
        У вас недостаточно прав для выполнения этой команды :/
        Если вы считаете, что необходимо разбанить какого либо пользователя, сообщите об этом администратору
        """;
    private const string SomeoneCallsAdmin = "{0}({1}) хочет разбанить пользователя на {2} курсе в {3} группе";
    private const string EnterUnbanInfoMessage =
        """
        Введите имя пользователя и название дисциплины с новой строки. Пример:
        username
        subject_name
        """;
    private const string UnbanInfoInvalidFormatMessage = "Данные введены в некорректном формате";
    private const string InvalidInfoMessage = "Введены некорректные данные:\n{0}";
    private const string UserToUnbanNotFoundMessage = "Пользователя с именем {0} не существует";
    private const string YouAreWhoYouAre = "Забанить самого себя нельзя. Смирись с этим...";
    private const string SubjectNotFountMessage = "Дисциплины {0} не существует на курсе выбранного пользователя";
    private const string AlreadyUnbannedMessage = "Пользователь {0} не находится в черном списке по дисциплине {1}";
    private const string SuccessUnbanMessage = "Пользователь {0} успешно разабанен по дисциплние {1}";

    private readonly Regex _unbanInfoPattern = new(@"^@(\w+)\n(.+)");
    
    public override string Type => commandsOptions.Value.Unban.Type;
    public override string Name => commandsOptions.Value.Unban.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.Unban, UpdateType.Message) ];
    public override Role AcceptRole => Role.Privileged;
    public override string Definition => commandsOptions.Value.Unban.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        switch (user.State)
        {
            case UserState.None:
            {
                switch (user.Role)
                {
                    case Role.Privileged:
                        await SendCallAdmin(botClient, user, cancellationToken);
                        break;
                    case Role.Admin:
                        await SendEnterUnbanInfo(botClient, user, cancellationToken);
                        break;
                }
                isSuccess = true;
                
                break;
            }
            case UserState.Unban:
            {
                await UnbanChosenUser(botClient, update, user, cancellationToken);
                isSuccess = true;
                
                break;
            }
        }
        
        return isSuccess;
    }
    
    private Task SendCallAdmin(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var sendMessages = new List<Task>
        {
            Task.Run(() =>
                    botClient.SendTextMessageAsync(
                        chatId: user.Id, 
                        text: CallAdminMessage, 
                        cancellationToken: cancellationToken),
                cancellationToken)
        };
        
        var adminMessage = string.Format(SomeoneCallsAdmin, user.Name, user.Username, user.CourseNumber, user.GroupNumber);
        sendMessages.AddRange(
            botOptions.Value.AdminChatId.Select(
                adminId => Task.Run(() =>
                        botClient.SendTextMessageAsync(
                            chatId: adminId,
                            text: adminMessage,
                            cancellationToken: cancellationToken), 
                    cancellationToken)));

        Task.WaitAll(sendMessages.ToArray(), cancellationToken);
        return Task.CompletedTask;
    }
    
    private async Task SendEnterUnbanInfo(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.Unban;
        await userRepository.SaveAsync(user, cancellationToken);
        
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: EnterUnbanInfoMessage,
            cancellationToken: cancellationToken);
    }

    private async Task UnbanChosenUser(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);
        
        var banInfo = update.Message?.Text;
        if (banInfo == null || !_unbanInfoPattern.IsMatch(banInfo))
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: UnbanInfoInvalidFormatMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var splitBanInfo = banInfo.Split('\n');
        var username = splitBanInfo[0];
        var subjectName = splitBanInfo[1];
        
        // валидация введенного username
        var usernameValidationResult = UserInfoValidator.ValidateUsername(username);
        if (usernameValidationResult != null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(InvalidInfoMessage, usernameValidationResult),
                cancellationToken: cancellationToken);
            return;
        }
        
        var userToUnban = (await userRepository.GetByConditionAsync(u => u.Username == username, cancellationToken: cancellationToken))
            .FirstOrDefault();
        
        // проверка существования введенного username
        if (userToUnban == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(UserToUnbanNotFoundMessage, username),
                cancellationToken: cancellationToken);
            return;
        }

        if (user.Id == userToUnban.Id)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: YouAreWhoYouAre,
                cancellationToken: cancellationToken);
            return;
        }

        var subject = await subjectRepository.GetByGroupAndName(userToUnban.CourseNumber, userToUnban.GroupNumber, subjectName, cancellationToken);

        // проверка существования введенного subjectName
        if (subject == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(SubjectNotFountMessage, subjectName),
                cancellationToken: cancellationToken);
            return;
        }

        var result = await blackListManagementService.UnbanBySubject(userToUnban.Id, subject.Id, cancellationToken);
        if (!result)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(AlreadyUnbannedMessage, username, subjectName),
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: string.Format(SuccessUnbanMessage, username, subjectName),
            cancellationToken: cancellationToken);
    }
}