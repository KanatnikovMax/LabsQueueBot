using System.Text;
using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Repository.Repository;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class ShowSubjectsCommandExecutor(
    ISubjectRepository subjectsRepository,
    ISerialNumberRepository serialNumberRepository,
    CommandsSettings commandsSettings) : ICommandExecutor // TODO проверить
{
    private string NoSubjectsMessage =
        $"В вашей группе не открыто ни одной очереди.\n{commandsSettings.JoinCommand.Name}"
        + " чтобы создать очередь";

    private const string OutOfSubject = "отсутствует";
    private const string Waiting = "в ожидании";
    public string Type => commandsSettings.ShowSubjectsCommand.Type;
    public string Name => commandsSettings.ShowSubjectsCommand.Name;
    public IReadOnlyCollection<UserState> States => [];
    public Role AcceptRole => Role.Default;
    public string Definition => commandsSettings.ShowSubjectsCommand.Definition;

    public async Task Execute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var subjects = (await subjectsRepository.GetByConditionAsync(s =>
                s.CourseNumber == user.CourseNumber
                && s.GroupNumber == user.GroupNumber,
            cancellationToken))
            .ToList();
        
        if (subjects.Count == 0)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: NoSubjectsMessage,
                cancellationToken: cancellationToken);
            return;
        }

        var serialNumbers = (await serialNumberRepository.GetByConditionAsync(sn => sn.TgUserIndex == user.Id, cancellationToken))
            .ToList();
        var actualSubjects = serialNumbers.Select(sn => sn.SubjectId).ToList();
        
        var builder = new StringBuilder();
        {
            builder.AppendLine("Очереди твоего курса и твои номера в них:");
            foreach (var subject in subjects)
            {
                string position;
                if (actualSubjects.Contains(subject.Id))
                {
                    var index = 1 + serialNumbers.First(sn => sn.SubjectId == subject.Id).QueueIndex;
                    position = index > 0
                        ? index.ToString()
                        : Waiting;
                }
                else
                {
                    position = OutOfSubject;
                }
                builder.AppendLine($"{subject.SubjectName} -> {position}");
            }

            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: builder.ToString(),
                cancellationToken: cancellationToken);
        }
    }
}