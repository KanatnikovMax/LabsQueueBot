using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Validators;
using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Commands.Implements;

public class AddSubjectCommandExecutor(
    IQueueInfoNotificationService queueInfoNotificationService,
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    IOptions<CommandsSettings> options,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string AddingSubjectMessage = "Введите название дисциплины, которую хотите добавить";
    private const string InvalidSubjectNameMessage = "Некорректное название дисциплины:\n{0}";
    private const string SubjectAlreadyExistsMessage = "Дисциплина с таким названием уже существует";
    private const string AddSubjectCompleteMessage = "Новая дисциплина успешно добавлена: {0}";
    
    public override string Type => options.Value.AddSubject.Type;
    public override string Name => options.Value.AddSubject.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.AddSubject, UpdateType.Message) ];
    public override Role AcceptRole => Role.Default;
    public override string Definition => options.Value.AddSubject.Definition;

    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        var isSuccess = false;
        switch (user.State)
        {
            case UserState.None:
            {
                await SendAddSubjectMessage(botClient, user, cancellationToken);
                isSuccess = true;

                break;
            }
            case UserState.AddSubject:
            {
                await AddNewSubject(botClient, update, user, cancellationToken);
                isSuccess = true;

                break;
            }
        }
        
        return isSuccess;
    }

    private async Task SendAddSubjectMessage(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken)
    {
        user.State = UserState.AddSubject;
        await userRepository.SaveAsync(user, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: AddingSubjectMessage,
            cancellationToken: cancellationToken);
    }

    private async Task AddNewSubject(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);
        
        var subjectName = update.Message!.Text;
        var validationResult = SubjectInfoValidator.ValidateSubjectName(subjectName);
        if (validationResult != null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(InvalidSubjectNameMessage, validationResult),
                cancellationToken: cancellationToken);
            return;
        }
        
        var subject = await subjectsRepository.GetByGroupAndName(user.CourseNumber, user.GroupNumber, subjectName!, cancellationToken);
        if (subject != null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: SubjectAlreadyExistsMessage,
                cancellationToken: cancellationToken);
            return;
        }

        subject = new Subject
        {
            CourseNumber = user.CourseNumber,
            GroupNumber = user.GroupNumber,
            SubjectName = subjectName!
        };
        subject = await subjectsRepository.SaveAsync(subject, cancellationToken);
        
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: string.Format(AddSubjectCompleteMessage, subjectName),
            cancellationToken: cancellationToken);

        await queueInfoNotificationService.NotifyGroupBySubject(user.CourseNumber, user.GroupNumber, subjectName!, 
            subject.Queue.ToList(), subject.Waiting.ToList(), cancellationToken);
    }
}