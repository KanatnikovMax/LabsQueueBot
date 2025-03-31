using LabsQueueBot.Db.Entities;

namespace LabsQueueBot.Commands;

public class CommandFactory
{
    private readonly IEnumerable<ICommand> _commands;

    public CommandFactory(IEnumerable<ICommand> commands)
    {
        _commands = commands;
    }

    public ICommand? GetCommand(string message)
    {
        return _commands.FirstOrDefault(c => c.Name.Equals(message));
    }

    public ICommand? GetCommand(User.UserState state)
    {
        return _commands.FirstOrDefault(c => c.State.Equals(state));
    }
}