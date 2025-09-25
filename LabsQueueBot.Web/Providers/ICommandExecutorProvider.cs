using LabsQueueBot.Core.Enums;
using LabsQueueBot.Web.Commands;
using Telegram.Bot.Types.Enums;

namespace LabsQueueBot.Web.Providers;

public interface ICommandExecutorProvider
{
    ICommandExecutor? GetCommandExecutorByText(string message, Role role);
    ICommandExecutor? GetCommandExecutorByState(UserState userState, UpdateType updateType, Role role);
}