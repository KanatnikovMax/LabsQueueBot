using System.Text.RegularExpressions;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Validators;
using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Repository.Repository;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class BanCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectRepository,
    IBlackListRepository blackListRepository,
    IOptions<TelegramBotSettings> botOptions,
    IOptions<CommandsSettings> commandsOptions,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string CallAdminMessage =
        """
        У вас недостаточно прав для выполнения этой команды :/
        Если вы считаете, что необходимо забанить какого либо пользователя, сообщите об этом администратору
        """;
    private const string SomeoneCallsAdmin = "{0}({1}) хочет забанить пользователя на {2} курсе в {3} группе";
    private const string EnterBanInfoMessage =
        """
        Введите имя пользователя, название дисциплины и продолжительность бана с новой строки. Пример:
        username
        subject_name
        timeout
        """;
    private const string BanInfoInvalidFormatMessage = "Данные введены в некорректном формате";
    private const string InvalidInfoMessage = "Введены некорректные данные:\n{0}";
    private const string UserToBanNotFoundMessage = "Пользователя с именем {0} не существует";
    private const string YouAreWhoYouAre = "Забанить самого себя нельзя. Ты тот, кто ты есть, смирись с этим.";
    private const string OutOfTimeoutMessage = "Нельзя забанить пользователя на большее число дней, чем {0}";
    private const string SubjectNotFountMessage = "Дисциплины {0} не существует на курсе выбранного пользователя";
    private const string AlreadyBanedMessage =
        """
        Пользователь {0} уже находится в черном списке по дисциплине {1}.
        Бан истекает {2} в {3}
        """;
    private const string SuccessBanMessage = "Пользователь {0} успешно забанен по дисциплние {1} на {2} дней";

    private readonly Regex _banInfoPattern = new(@"^@(\w+)\n(.+)\n(\d+)$");

    public override string Name => commandsOptions.Value.Ban.Name;
    public override string Type => commandsOptions.Value.Ban.Type;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows { get; } = [ (UserState.Ban, UpdateType.Message) ];
    public override Role AcceptRole => Role.Privileged;
    public override string Definition => commandsOptions.Value.Ban.Definition;
    
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
                        await SendEnterBanInfo(botClient, user, cancellationToken);
                        break;
                }
                isSuccess = true;

                break;
            } 
            case UserState.Ban:
            {
                await BanChosenUserBySubjectAndTimeout(botClient, update, user, cancellationToken);
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
    
    private async Task SendEnterBanInfo(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.Ban;
        await userRepository.SaveAsync(user, cancellationToken);
        
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: EnterBanInfoMessage,
            cancellationToken: cancellationToken);
    }

    private async Task BanChosenUserBySubjectAndTimeout(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);
        
        var banInfo = update.Message?.Text;
        if (banInfo == null || !_banInfoPattern.IsMatch(banInfo))
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: BanInfoInvalidFormatMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var splitBanInfo = banInfo.Split('\n');
        var username = splitBanInfo[0];
        var subjectName = splitBanInfo[1];
        var timeout = int.Parse(splitBanInfo[2]);

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

        // валидация введенного timeout
        if (timeout > botOptions.Value.MaxBanTimeoutInDays)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(OutOfTimeoutMessage, botOptions.Value.MaxBanTimeoutInDays),
                cancellationToken: cancellationToken);
            return;
        }
        
        var userToBan = (await userRepository.GetByConditionAsync(u => u.Username == username, cancellationToken: cancellationToken))
            .FirstOrDefault();
        
        // проверка существования введенного username
        if (userToBan == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(UserToBanNotFoundMessage, username),
                cancellationToken: cancellationToken);
            return;
        }

        if (user.Id == userToBan.Id)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: YouAreWhoYouAre,
                cancellationToken: cancellationToken);
            return;
        }

        var subject = await subjectRepository.GetByGroupAndName(userToBan.CourseNumber, userToBan.GroupNumber, subjectName, cancellationToken);

        // проверка существования введенного subjectName
        if (subject == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(SubjectNotFountMessage, subjectName),
                cancellationToken: cancellationToken);
            return;
        }

        var baned = await blackListRepository.GetBanByUserAndSubject(userToBan.Id, subject.Id, cancellationToken);

        // проверяем, что если пользователь уже был забанен и время бана еще не вышло
        if (baned != null && baned.UnbanDate > DateTime.UtcNow)
        {
            // переводим из utc в местное время
            var localUnbanDate = baned.UnbanDate.Value.AddHours(botOptions.Value.LocalUtcOffset);
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(AlreadyBanedMessage, username, subjectName, localUnbanDate.Date.ToString("dd/MM/yyyy"), localUnbanDate.TimeOfDay.ToString("hh\\:mm")),
                cancellationToken: cancellationToken);
            return;
        }
        
        // удаляем пользователя из очереди ожидания
        subject.Waiting = subject.Waiting.Where(x => x != userToBan.Id).ToArray();

        // обновляем информацию о бане
        baned ??= new Baned
        {
            UserId = userToBan.Id,
            SubjectId = subject.Id
        };
        baned.ExecutorId = user.Id;
        baned.UnbanDate = DateTime.UtcNow + TimeSpan.FromDays(timeout);

        Task.WaitAll([
                subjectRepository.SaveAsync(subject, cancellationToken),
                blackListRepository.SaveAsync(baned, cancellationToken)
            ], cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: string.Format(SuccessBanMessage, username, subjectName, timeout),
            cancellationToken: cancellationToken);
    }
}