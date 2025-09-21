using System.Security.Cryptography;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.DataAccess.Entities;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class UnionQueueCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    ISerialNumberRepository serialNumberRepository,
    CommandsSettings commandsSettings) : ICommandExecutor // TODO проверить
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string WrongUserAcceptRole = """
                                               У вас недостаточно прав для этого действия.
                                               Если вы так не считаете - обратитесь к администратору
                                               """;
    private const string WrongCallbackQueryMessageRequest = "Не в той табличке ты тыкнул";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    private const string WaitingListIsEmpty = "Список ожидания по выбранному предмету пуст";
    private const string UnionCompleteMessage = "Очередь по выбранному предмету сформирована:";
    public string Type => commandsSettings.UnionQueueCommand.Type;
    public string Name => commandsSettings.UnionQueueCommand.Name;
    public IReadOnlyCollection<UserState> States => [UserState.Union];
    public Role AcceptRole => Role.Privileged;
    public string Definition => commandsSettings.UnionQueueCommand.Definition;

    public async Task Execute(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        switch (user.State)
        {
            case UserState.None:
            {
                user.State = UserState.Union;
                await userRepository.SaveAsync(user, cancellationToken);

                await SendSubjectsKeyboard(botClient, user, cancellationToken);

                return;
            }
            case UserState.Join:
            {
                if (!update.Type.Equals(UpdateType.CallbackQuery))
                {
                    await botClient.DeleteMessageAsync(
                        chatId: user.Id,
                        messageId: update.Message.MessageId,
                        cancellationToken: cancellationToken);
                    return;
                }

                await UnionQueue(botClient, update, user, cancellationToken);
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

    private async Task UnionQueue(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(
            chatId: user.Id,
            messageId: update.CallbackQuery.Message.MessageId,
            cancellationToken: cancellationToken);

        if (user.Role < AcceptRole)
        {
            user.State = UserState.None;
            await userRepository.SaveAsync(user, cancellationToken);

            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: WrongUserAcceptRole,
                cancellationToken: cancellationToken);
        }

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

        if (waiting.Count == 0)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: WaitingListIsEmpty,
                cancellationToken: cancellationToken);
            return;
        }

        var queue = (await serialNumberRepository.GetQueueBySubject(subject, cancellationToken)).ToList();
        var lastSn = queue.MaxBy(sn => sn.QueueIndex);

        waiting = RandomizeSerialNumbers(lastSn?.QueueIndex ?? 0, waiting);
        await serialNumberRepository.SaveRange(waiting, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: $"{UnionCompleteMessage} {subject.SubjectName}",
            cancellationToken: cancellationToken);
    }

    private static List<SerialNumber> RandomizeSerialNumbers(int lastNumber, List<SerialNumber> waiting)
    {
        var result = new List<SerialNumber>();

        while (waiting.Count > 0)
        {
            var index = RandomNumberGenerator.GetInt32(0, waiting.Count);
            waiting[index].QueueIndex = ++lastNumber;

            result.Add(waiting[index]);
            waiting.RemoveAt(index);
        }

        return result;
    }
}