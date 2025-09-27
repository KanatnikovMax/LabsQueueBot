using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Helpers;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Repository.Repository;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;
using User = LabsQueueBot.DataAccess.Entities.User;

namespace LabsQueueBot.Web.Commands.Implements;

public class ShowSubjectsCommandExecutor(
    ISubjectRepository subjectRepository,
    IOptions<CommansSettings> options,
    ILogger logger) : CommandExecutorBase(logger), ICommandExecutor
{
    private const string NoSubjectsMessage = """
                                       В вашей группе не открыто ни одной очереди :/
                                       {0} чтобы добавить дисциплину и {1} чтобы встать в очередь
                                       """;
    
    public override string Type => options.Value.ShowSubjects.Type;
    public override string Name => options.Value.ShowSubjects.Name;
    public override IReadOnlyCollection<(UserState State, UpdateType Type)> Allows => [];
    public override Role AcceptRole => Role.Default;
    public override string Definition => options.Value.ShowSubjects.Definition;

    protected override async Task<bool> InternalExecute(ITelegramBotClient botClient, Update update, User user, CancellationToken cancellationToken)
    {
        var subjects = (await subjectRepository.GetByGroup(user.CourseNumber, user.GroupNumber, cancellationToken))
            .ToDictionary(s => s.SubjectName, s => (s.Queue.ToList(), s.Waiting.ToList()));

        if (subjects.Count == 0)
        {
            await botClient.SendTextMessageAsync(
                chatId: user.Id,
                text: string.Format(NoSubjectsMessage, options.Value.AddSubject.Name, options.Value.Join.Name),
                cancellationToken: cancellationToken);
            
            return false;
        }
        
        var message = QueueInfoBuildHelper.GetByUser(user.Id, subjects);
        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: message,
            cancellationToken: cancellationToken);

        return true;
    }
}