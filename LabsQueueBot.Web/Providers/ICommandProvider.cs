using LabsQueueBot.Core.Enums;
using LabsQueueBot.Web.Commands;

namespace LabsQueueBot.Web.Providers;

public interface ICommandProvider
{
    ICommandExecutor? GetCommandByText(string message, Role role);
    ICommandExecutor? GetCommandByState(UserState userState, Role role);
}