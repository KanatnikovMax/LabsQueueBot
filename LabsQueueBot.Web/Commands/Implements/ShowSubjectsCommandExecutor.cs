using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Repository.Repository;
using Telegram.Bot;
using Telegram.Bot.Types;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class ShowSubjectsCommandExecutor(
    ISubjectRepository subjectRepository,
    CommandsSettings commandsSettings,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string NoSubjectsMessage = """
                                       В вашей группе не открыто ни одной очереди :/
                                       {0} чтобы добавить дисциплину и {1} чтобы встать в очередь
                                       """;
    
    public override string Type => commandsSettings.ShowSubjectsCommand.Type;
    public override string Name => commandsSettings.ShowSubjectsCommand.Name;
    public override IReadOnlyCollection<UserState> States => [];
    public override Role AcceptRole => Role.Default;
    public override string Definition => commandsSettings.ShowSubjectsCommand.Definition;

    protected override async Task InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var subjects = (await subjectRepository.GetByGroup(user.CourseNumber, user.GroupNumber, cancellationToken))
            .ToDictionary(s => s.SubjectName, s => (s.Queue.ToList(), s.Waiting.ToList()));

        if (subjects.Count == 0)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(NoSubjectsMessage, commandsSettings.AddSubjectCommand.Name, commandsSettings.JoinCommand.Name),
                cancellationToken: cancellationToken);
            return;
        }
        
        var message = QueueInfoBuildHelper.GetByUser(user.Id, subjects);
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: message,
            cancellationToken: cancellationToken);
    }
}