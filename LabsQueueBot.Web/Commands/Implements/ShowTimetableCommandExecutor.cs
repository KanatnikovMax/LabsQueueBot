using System.Text;
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

public class ShowTimetableCommandExecutor(
    IUserRepository userRepository,
    ISubjectRepository subjectsRepository,
    CommandsSettings commandsSettings) : ICommandExecutor // TODO проверить
{
    private const string SendSubjectsKeyboardMessage = "Выберите дисциплину:";
    private const string WrongCallbackQueryMessageRequest = "Не в той табличке ты тыкнул";
    private const string SubjectNotFoundMessage = "Такой дисциплины не существует";
    public string Type => commandsSettings.ShowTimetableCommand.Type;
    public string Name => commandsSettings.ShowTimetableCommand.Name;
    public IReadOnlyCollection<UserState> States => [UserState.ShowTimetable];
    public Role AcceptRole => Role.Default;
    public string Definition => commandsSettings.ShowTimetableCommand.Definition;

    public async Task Execute(ITelegramBotClient botClient, Update update, User user,
        CancellationToken cancellationToken)
    {
        /*
        switch (user.State)
        {
            case UserState.None:
            {
                user.State = UserState.ShowTimetable;
                await userRepository.SaveAsync(user, cancellationToken);

                await SendSubjectsKeyboard(botClient, user, cancellationToken);

                return;
            }
            case UserState.ShowTimetable:
            {
                if (!update.Type.Equals(UpdateType.CallbackQuery))
                {
                    await botClient.DeleteMessageAsync(
                        chatId: user.Id,
                        messageId: update.Message.MessageId,
                        cancellationToken: cancellationToken);
                    return;
                }

                await SendSubjectTimetable(botClient, update, user, cancellationToken);
                return;
            }
        }
        */
    }
    /*
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

    private async Task SendSubjectTimetable(ITelegramBotClient botClient, Update update, User user,
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
                cancellationToken))
            .FirstOrDefault();
        
        if (subject is null)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: SubjectNotFoundMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var subjectTimetable = CreateTimetable(subject);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: subjectTimetable,
            cancellationToken: cancellationToken);
    }

    private static string CreateTimetable(Subject subject)
    {
        var builder = new StringBuilder();

        builder.AppendLine(subject.SubjectName);

        var oddRawTimetable = subject.DenWeekTimetableMask.Split("");
        if (subject.DenWeekTimetableMask != subject.NumWeekTimetableMask)
        {
            builder.AppendLine($"Числитель: {ParseTimetable(oddRawTimetable)}");

            var evenRawTimetable = subject.NumWeekTimetableMask.Split("");
            builder.AppendLine($"Знаменатель: {ParseTimetable(evenRawTimetable)}");
        }
        else
        {
            builder.AppendLine(ParseTimetable(oddRawTimetable));
        }

        return builder.ToString();
    }

    private static string ParseTimetable(string[] rawDays)
    {
        var builder = new StringBuilder();

        foreach (var rawDay in rawDays)
        {
            builder.AppendJoin(' ', ParseDayOfWeek(int.Parse(rawDay)));
        }

        return builder.ToString();
    }

    private static string ParseDayOfWeek(int day)
    {
        return day switch
        {
            // 1 => "понедельник",
            // 2 => "вторник",
            // 3 => "среда",
            // 4 => "четверг",
            // 5 => "пятница",
            // 6 => "суббота",
            // 7 => "воскресенье",
            // _ => "воскресенье"
            1 => "пн",
            2 => "вт",
            3 => "ср",
            4 => "чт",
            5 => "пт",
            6 => "сб",
            7 => "вс",
            _ => "вс"
        };
    }
    */
}