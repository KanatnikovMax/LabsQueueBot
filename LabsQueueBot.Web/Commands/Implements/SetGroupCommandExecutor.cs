using System.Text.Json;
using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Utils;
using LabsQueueBot.Core.Validators;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class SetGroupCommandExecutor(
    IUserRepository userRepository,
    ISubjectsManagementService subjectsManagementService,
    IUserManagementService userManagementService,
    CommandsSettings commandsSettings, 
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string SendGroupsKeyboardMessage = "Выберите курс и группу:";
    private const string AddGroupMessage = "Введите курс и группу в формате course:group";
    private const string InvalidGroupInfoMessage = "Введены некорректные данные:\n{0}";
    private const string AlreadyInChosenGroupMessage = "Ты уже находишься в выбранной группе =)";
    private const string SuccessMessage = "Курс и группа успешно обновлены";
    
    public override string Type => commandsSettings.SetGroupCommand.Type;
    public override string Name => commandsSettings.SetGroupCommand.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [
        (UserState.ChooseGroup, UpdateType.CallbackQuery),
        (UserState.AddGroup, UpdateType.Message)
    ];
    public override Role AcceptRole => Role.Nobody;
    public override string Definition => commandsSettings.SetGroupCommand.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        switch (user.State)
        {
            case UserState.None:
            {
                if (update.Type == UpdateType.Message)
                {
                    await SendGroupsKeyboard(botClient, user, cancellationToken);
                    isSuccess = true;
                }
                break;
            }
            case UserState.ChooseGroup:
            {
                if (update.Type == UpdateType.CallbackQuery
                    && update.CallbackQuery!.Message!.MessageId == user.LastCallbackableMessageId)
                {
                    user.LastCallbackableMessageId = null;
                    await PutUserToChosenGroup(botClient, update, user, cancellationToken);
                    isSuccess = true;
                }
                break;
            }
            case UserState.AddGroup:
            {
                if (update.Type == UpdateType.Message)
                {
                    await PutUserToAddedGroup(botClient, update, user, cancellationToken);
                    isSuccess = true;
                }
                break;
            }
        }

        return isSuccess;
    }
    
    private async Task SendGroupsKeyboard(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var groups = (await userRepository.GetAllGroups(cancellationToken))
            .Select(x => x.course + " курс " + x.group + " группа")
            .ToList();
        
        var keyboard = InlineKeyboardHelper.ListToKeyboard(groups, true, true, 1);
        
        var message = await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SendGroupsKeyboardMessage,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.State = UserState.ChooseGroup;
        user.LastCallbackableMessageId = message.MessageId;
        await userRepository.SaveAsync(user, cancellationToken);
    }

    private async Task PutUserToChosenGroup(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        await BotClientUtils.ClearMarkupMessage(
            botClient: botClient,
            chatId: user.Id,
            messageId: update.CallbackQuery!.Message!.MessageId,
            message: $"{SendGroupsKeyboardMessage} {update.CallbackQuery.Data}",
            cancellationToken: cancellationToken);
        
        var subjectName = update.CallbackQuery.Data;
        
        if (subjectName == InlineKeyboardHelper.AddMessage)
        {
            user.State = UserState.AddGroup;
            await userRepository.SaveAsync(user, cancellationToken);
            
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: AddGroupMessage,
                cancellationToken: cancellationToken);
            return;
        }
        
        if (subjectName == InlineKeyboardHelper.BackMessage)
        {
            user.State = UserState.None;
            await userRepository.SaveAsync(user, cancellationToken);
            return;
        }
        
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);
        
        var validationResult = CourseGroupValidator.ValidateFormatted(update.CallbackQuery.Data);
        if (validationResult != null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(InvalidGroupInfoMessage, validationResult),
                cancellationToken: cancellationToken);
            return;
        }
        
        var courseGroup = ParseFormattedCourseGroup(update.CallbackQuery.Data!);
        
        if (courseGroup.Course == user.CourseNumber && courseGroup.Group == user.GroupNumber)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: AlreadyInChosenGroupMessage,
                cancellationToken: cancellationToken);
            return;
        }
        
        await subjectsManagementService.DeleteUserFromSubjectsQueues(user.Id, user.CourseNumber, user.GroupNumber, cancellationToken);
        
        await userManagementService.PutUserIntoGroup(user, courseGroup.Course, courseGroup.Group, cancellationToken);
        
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SuccessMessage,
            cancellationToken: cancellationToken);
    }
    
    private async Task PutUserToAddedGroup(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        var validationResult = CourseGroupValidator.Validate(update.Message!.Text);
        if (validationResult != null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(InvalidGroupInfoMessage, validationResult),
                cancellationToken: cancellationToken);
            return;
        }

        var courseGroup = ParseRawCourseGroup(update.Message.Text!);
        
        await subjectsManagementService.DeleteUserFromSubjectsQueues(user.Id, user.CourseNumber, user.GroupNumber, cancellationToken);
        
        await userManagementService.PutUserIntoGroup(user, courseGroup.Course, courseGroup.Group, cancellationToken);
        
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SuccessMessage,
            cancellationToken: cancellationToken);
    }

    private static (byte Course, byte Group) ParseFormattedCourseGroup(string info)
    {
        var parsed = info.Split(' ');
        return (byte.Parse(parsed[0]), byte.Parse(parsed[2]));
    }
    
    private static (byte Course, byte Group) ParseRawCourseGroup(string info)
    {
        var parsed = info.Split(':');
        return (byte.Parse(parsed[0]), byte.Parse(parsed[1]));
    }
}