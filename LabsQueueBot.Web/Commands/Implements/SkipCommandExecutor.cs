using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class SkipCommandExecutor(
    IUserRepository userRepository,
    ISerialNumberRepository serialNumberRepository,
    ISubjectRepository subjectsRepository,
    CommandsSettings commandsSettings) : ICommandExecutor // TODO проверить
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string WrongCallbackQueryMessageRequest = "Не в той табличке ты тыкнул";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    private const string UserIsWaitingMessage = "Ты в списке ожидания, так чего не ждётся?";
    private const string UserNotExistsInQueueMessage = "Вас нет в очереди по дисциплине ";
    private const string UserIsLastInQueue = "Ты уже итак в конце очереди, ожидай своего часа :)";
    private const string SkipCompleteMessage = "Это как шаг вперед, но назад";
    
    public string Type => commandsSettings.SkipCommand.Type;
    public string Name => commandsSettings.SkipCommand.Name;
    public IReadOnlyCollection<UserState> States => [UserState.Skip];
    public Role AcceptRole => Role.Default;
    public string Definition => commandsSettings.SkipCommand.Definition;
    public async Task Execute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        switch (user.State)
        {
            case UserState.None:
            {
                user.State = UserState.Skip;
                await userRepository.SaveAsync(user, cancellationToken);

                await SendSubjectsKeyboard(botClient, user, cancellationToken);

                return;
            }
            case UserState.Skip:
            {
                if (!update.Type.Equals(UpdateType.CallbackQuery))
                {
                    await botClient.DeleteMessageAsync(
                        chatId: user.Id,
                        messageId: update.Message.MessageId,
                        cancellationToken: cancellationToken);
                    return;
                }

                await SkipUserInQueue(botClient, update, user, cancellationToken);
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

    private async Task SkipUserInQueue(ITelegramBotClient botClient, Update update, User user,
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

        var subject = (await subjectsRepository.GetByConditionAsync(s =>
                s.CourseNumber == user.CourseNumber
                && s.GroupNumber == user.GroupNumber
                && s.SubjectName == subjectName,
            cancellationToken
        )).FirstOrDefault();

        if (subject is null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: SubjectNotFoundMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var waiting = (await serialNumberRepository.GetWaitingBySubject(subject, cancellationToken)).ToList();
        if (waiting.Select(sn => sn.TgUserIndex).Contains(user.Id))
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: UserIsWaitingMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var queue = (await serialNumberRepository.GetQueueBySubject(subject, cancellationToken)).ToList();
        if (!queue.Select(sn => sn.TgUserIndex).Contains(user.Id))
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: UserNotExistsInQueueMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var sn1 = queue.First(sn => sn.TgUserIndex == user.Id);
        var sn2 = queue.FirstOrDefault(sn => sn.QueueIndex > sn1.QueueIndex);
        
        if (sn2 is null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: UserIsLastInQueue,
                cancellationToken: cancellationToken);
            return;
        }

        await serialNumberRepository.SwapUsersInQueue(sn1, sn2, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: SkipCompleteMessage,
            cancellationToken: cancellationToken);
    }
}