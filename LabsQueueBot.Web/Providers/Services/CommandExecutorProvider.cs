using LabsQueueBot.Core.Enums;
using LabsQueueBot.Web.Commands;
using Telegram.Bot.Types.Enums;

namespace LabsQueueBot.Web.Providers.Services;

public class CommandExecutorProvider(IEnumerable<ICommandExecutor> commands) : ICommandExecutorProvider
{
    public ICommandExecutor? GetCommandExecutorByText(string message, Role role)
    {
        return commands.FirstOrDefault(c => c.Name == message && c.AcceptRole <= role);
    }

    public ICommandExecutor? GetCommandExecutorByState(UserState userState, UpdateType updateType, Role role)
    {
        return commands.FirstOrDefault(c => c.Allows.Contains((userState, updateType)) && c.AcceptRole <= role);
    }
}