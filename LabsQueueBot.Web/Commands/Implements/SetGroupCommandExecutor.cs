using System.Text.Json;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Utils;
using LabsQueueBot.Core.Validators;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class SetGroupCommandExecutor(
    IUserRepository userRepository,
    CommandsSettings commandsSettings, 
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string SendGroupsKeyboardMessage = "Выберите курс и группу:";
    private const string WrongCallbackQueryMessageRequest = "Не в той табличке ты тыкнул";
    private const string AddGroupMessage = "Введите курс и группу в формате course:group";
    private const string InvalidGroupInfoMessage = "Введены некорректные данные:\n{0}";
    private const string AlreadyInChosenGroupMessage = "Ты уже находишься в выбранной группе =)";
    private const string SuccessMessage = "Курс и группа успешно обновлены";
    
    public override string Type => commandsSettings.SetGroupCommand.Type;
    public override string Name => commandsSettings.SetGroupCommand.Name;
    public override IReadOnlyCollection<UserState> States => [UserState.ChooseGroup, UserState.AddGroup];
    public override Role AcceptRole => Role.Nobody;
    public override string Definition => commandsSettings.SetGroupCommand.Definition;
    
    protected override async Task InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        switch (user.State)
        {
            case UserState.None:
            {
                if (update.Type == UpdateType.Message)
                {
                    await SendGroupsKeyboard(botClient, user, cancellationToken);
                    return;
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
                    return;
                }
                break;
            }
            case UserState.AddGroup:
            {
                if (update.Type == UpdateType.Message)
                {
                    await PutUserToAddedGroup(botClient, update, user, cancellationToken);
                    return;
                }
                break;
            }
        }
        // если при  UserState.None, UserState.ChooseGroup или UserState.AddGroup получены Update не ожидаемого типа
        if (!await BotClientUtils.DeleteUpdate(botClient, user.Id, update, cancellationToken))
        {
            // в случае если получили невозможный Update (не Message и не CallbackQuery) - игнорируем его
            var updateString = JsonSerializer.Serialize(update);
            logger.Warning("Update.MessageId is null\n\n{updateString}", updateString);
        }
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
        
        if (subjectName == InlineKeyboardHelper.BackMessage)
        {
            user.State = UserState.None;
            await userRepository.SaveAsync(user, cancellationToken);
            return;
        }
        
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
        
        if (courseGroup.course == user.CourseNumber && courseGroup.group == user.GroupNumber)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: AlreadyInChosenGroupMessage,
                cancellationToken: cancellationToken);
            return;
        }
        
        await SaveChosenCourseGroup(courseGroup, user, cancellationToken);
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

        var courseGroup = ParseCourseGroup(update.Message.Text!);
        await SaveChosenCourseGroup(courseGroup, user, cancellationToken);
        
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SuccessMessage,
            cancellationToken: cancellationToken);
    }
    
    private async Task SaveChosenCourseGroup((byte course, byte group) courseGroup, User user,
        CancellationToken cancellationToken)
    {
        user.State = UserState.None;
        user.Role = Role.Default;
        user.CourseNumber = courseGroup.course;
        user.GroupNumber = courseGroup.group;
        
        await userRepository.SaveAsync(user, cancellationToken);
    }

    private static (byte course, byte group) ParseFormattedCourseGroup(string info)
    {
        var parsed = info.Split(' ');
        return (byte.Parse(parsed[0]), byte.Parse(parsed[2]));
    }
    
    private static (byte course, byte group) ParseCourseGroup(string info)
    {
        var parsed = info.Split(':');
        return (byte.Parse(parsed[0]), byte.Parse(parsed[1]));
    }
}