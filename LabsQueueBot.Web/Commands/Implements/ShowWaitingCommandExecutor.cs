using System.Text;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Core.Utils;
using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class ShowWaitingCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    ISerialNumberRepository serialNumberRepository,
    CommandsSettings commandsSettings,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor // TODO не используется: функциональность вынесена в ShowQueue
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    
    public override string Type => commandsSettings.ShowWaitingCommand.Type;
    public override string Name => commandsSettings.ShowWaitingCommand.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [ (UserState.ShowWaiting, UpdateType.CallbackQuery) ];
    public override Role AcceptRole => Role.Default;
    public override string Definition => commandsSettings.ShowWaitingCommand.Definition;
    
    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        switch (user.State)
        {
            case UserState.None:
            {
                if (update.Type == UpdateType.Message)
                {
                    await SendSubjectsKeyboard(botClient, user, cancellationToken);
                    isSuccess = true;
                }
                break;
            }
            case UserState.ShowWaiting:
            {
                if (update.Type == UpdateType.CallbackQuery
                    && update.CallbackQuery!.Message!.MessageId == user.LastCallbackableMessageId)
                {
                    user.LastCallbackableMessageId = null;
                    await SendWaitingList(botClient, update, user, cancellationToken);
                    isSuccess = true;
                }
                break;
            }
        }
        
        return isSuccess;
    }
    
    private async Task SendSubjectsKeyboard(ITelegramBotClient botClient, User user, 
        CancellationToken cancellationToken)
    {
        var subjects = (await subjectsRepository.GetByConditionAsync(s =>
                    s.CourseNumber == user.CourseNumber && s.GroupNumber == user.GroupNumber,
                cancellationToken))
            .Select(s => s.SubjectName).ToList();
        var keyboard = InlineKeyboardHelper.ListToKeyboard(subjects, false, true, 1);

        var message = await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SendSubjectsKeyboardMessage,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
        
        user.State = UserState.ShowQueue;
        user.LastCallbackableMessageId = message.MessageId;
        await userRepository.SaveAsync(user, cancellationToken);
    }
    
    private async Task SendWaitingList(ITelegramBotClient botClient, Update update, User user, 
        CancellationToken cancellationToken)
    {
        await BotClientUtils.ClearMarkupMessage(
            botClient: botClient,
            chatId: user.Id,
            messageId: update.CallbackQuery!.Message!.MessageId,
            message: $"{SendSubjectsKeyboardMessage} {update.CallbackQuery.Data}",
            cancellationToken: cancellationToken);

        var subjectName = update.CallbackQuery.Data;
        
        if (subjectName == InlineKeyboardHelper.BackMessage)
        {
            user.State = UserState.None;
            await userRepository.SaveAsync(user, cancellationToken);
            return;
        }
        
        user.State = UserState.None;
        await userRepository.SaveAsync(user, cancellationToken);
        
        var subject = await subjectsRepository.GetByGroupAndName(user.CourseNumber, user.GroupNumber, subjectName!, cancellationToken);

        if (subject is null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: SubjectNotFoundMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var queueList = await CreateWaitingList(subject, cancellationToken);
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: queueList,
            cancellationToken: cancellationToken);
    }

    private async Task<string> CreateWaitingList(Subject subject, CancellationToken cancellationToken)
    {
        var waiting = (await serialNumberRepository.GetWaitingBySubject(subject, cancellationToken)).ToList();// TODO убрать serialNumberRepository
                
        var builder = new StringBuilder();
        builder.AppendLine($"Текущая очередь по дисциплине {subject.SubjectName}:");
        if (waiting.Count > 0)
        {
            foreach (var sn in waiting)
            {
                var queueUser = await userRepository.GetByIdAsync(sn.TgUserIndex, cancellationToken); 
                builder.AppendLine($"{queueUser.Name}");
            }
        }
        else
        {
            builder.AppendLine("Пуста");
        }

        return builder.ToString();
    }
}