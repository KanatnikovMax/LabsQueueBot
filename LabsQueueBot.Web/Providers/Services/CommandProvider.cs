using LabsQueueBot.Core.Enums;
using LabsQueueBot.Web.Commands;

namespace LabsQueueBot.Web.Providers.Services;

public class CommandProvider(IEnumerable<ICommandExecutor> commands) : ICommandProvider
{
    public ICommandExecutor? GetCommandByText(string message, Role role)
    {
        return commands.FirstOrDefault(c => c.Name == message && c.AcceptRole <= role);
    }

    public ICommandExecutor? GetCommandByState(UserState userState, Role role)
    {
        return commands.FirstOrDefault(c => c.States.Contains(userState) && c.AcceptRole <= role);
    }
}