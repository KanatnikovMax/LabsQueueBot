using System.Text;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Web.Settings.Commands;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using LabsQueueBot.Web.Providers;
using LabsQueueBot.Web.Providers.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class ShowQueueCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    ISerialNumberRepository serialNumberRepository,
    CommandsSettings commandsSettings) : ICommandExecutor // TODO проверить
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string WrongCallbackQueryMessageRequest = "Не в той табличке ты тыкнул";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    public string Type => commandsSettings.ShowQueueCommand.Type;
    public string Name => commandsSettings.ShowQueueCommand.Name;
    public IReadOnlyCollection<UserState> States => [UserState.ShowQueue];
    public Role AcceptRole => Role.Default;
    public string Definition => commandsSettings.ShowQueueCommand.Definition;

    public async Task Execute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        switch (user.State)
        {
            case UserState.None:
            {
                user.State = UserState.ShowQueue;
                await userRepository.SaveAsync(user, cancellationToken);
                
                await SendSubjectsKeyboard(botClient, user, cancellationToken);
                
                return;
            }
            case UserState.ShowQueue:
            {
                if (!update.Type.Equals(UpdateType.CallbackQuery))
                {
                    await botClient.DeleteMessageAsync(
                        chatId: user.Id,
                        messageId: update.Message.MessageId,
                        cancellationToken: cancellationToken);
                    return;
                }
                
                await SendQueueList(botClient, update, user, cancellationToken);
                return;
            }
        }
    }

    private async Task SendSubjectsKeyboard(ITelegramBotClient botClient, User user, 
        CancellationToken cancellationToken)
    {
        var subjects = (await subjectsRepository.GetByConditionAsync(s =>
                    s.CourseNumber == user.CourseNumber && s.GroupNumber == user.GroupNumber,
                cancellationToken))
            .Select(s => s.SubjectName).ToList();
        var keyboard = InlineKeyboardHelper.ListToKeyboard(subjects, false, true, 1);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SendSubjectsKeyboardMessage,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task SendQueueList(ITelegramBotClient botClient, Update update, User user, 
        CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(
            chatId: user.Id,
            messageId: update.CallbackQuery.Message.MessageId,
            cancellationToken: cancellationToken);
        
        if (update.CallbackQuery.Message.Text != SendSubjectsKeyboardMessage)
        {
            user.State = UserState.None;
            await userRepository.SaveAsync(user, cancellationToken);
            
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: WrongCallbackQueryMessageRequest,
                cancellationToken: cancellationToken);
            return;
        }

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

        var queueList = await CreateQueueList(subject, cancellationToken);
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: queueList,
            cancellationToken: cancellationToken);
    }

    private async Task<string> CreateQueueList(Subject subject, CancellationToken cancellationToken)
    {
        var queue = (await serialNumberRepository.GetQueueBySubject(subject, cancellationToken)).ToList();
                
        var builder = new StringBuilder();
        builder.AppendLine($"Текущая очередь по дисциплине {subject.SubjectName}:");
        if (queue.Count > 0)
        {
            queue.Sort((x, y) => x.QueueIndex - y.QueueIndex);
                    
            var number = 1;
            foreach (var sn in queue)
            {
                var queueUser = await userRepository.GetByIdAsync(sn.TgUserIndex, cancellationToken); 
                builder.AppendLine($"{number++}. {queueUser.Name}");
            }
        }
        else
        {
            builder.AppendLine("Пуста");
        }

        return builder.ToString();
    }
}