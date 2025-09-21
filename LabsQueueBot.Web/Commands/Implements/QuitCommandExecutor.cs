using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Web.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class QuitCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    ISerialNumberRepository serialNumberRepository,
    CommandsSettings commandsSettings) : ICommandExecutor // TODO проверить
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string WrongCallbackQueryMessageRequest = "Не в той табличке ты тыкнул";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    private const string UserNotExistsInQueueMessage = "Вас нет в очереди по дисциплине ";
    private const string QuitCompleteMessage = "Вы вышли из очереди по дисциплине ";
    public string Type => commandsSettings.QuitCommand.Type;
    public string Name => commandsSettings.QuitCommand.Name;
    public IReadOnlyCollection<UserState> States => [UserState.Quit];
    public Role AcceptRole => Role.Default;
    public string Definition => commandsSettings.QuitCommand.Definition;

    public async Task Execute(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        switch (user.State)
        {
            case UserState.None:
            {
                user.State = UserState.Quit;
                await userRepository.SaveAsync(user, cancellationToken);

                await SendSubjectsKeyboard(botClient, user, cancellationToken);

                return;
            }
            case UserState.Quit:
            {
                if (!update.Type.Equals(UpdateType.CallbackQuery))
                {
                    await botClient.DeleteMessageAsync(
                        chatId: user.Id,
                        messageId: update.Message.MessageId,
                        cancellationToken: cancellationToken);
                    return;
                }

                await QuitUserFromQueue(botClient, update, user, cancellationToken);
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

    private async Task QuitUserFromQueue(ITelegramBotClient botClient, Update update, User user,
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

        var sn = (await serialNumberRepository.GetQueueBySubject(subject, cancellationToken))
                 .FirstOrDefault(sn => sn.TgUserIndex == user.Id)
                 ??
                 (await serialNumberRepository.GetWaitingBySubject(subject, cancellationToken))
                 .FirstOrDefault(sn => sn.TgUserIndex == user.Id);

        if (sn is not null)
        {
            await serialNumberRepository.DeleteAsync(sn, cancellationToken);

            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: $"{QuitCompleteMessage} {subject.SubjectName}",
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: $"{UserNotExistsInQueueMessage} {subject.SubjectName}",
            cancellationToken: cancellationToken);
    }
}