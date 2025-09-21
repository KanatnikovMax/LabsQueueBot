using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.Commands;
using LabsQueueBot.Web.Commands.Implements;
using LabsQueueBot.Web.Providers;
using LabsQueueBot.Web.Providers.Services;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class CommandsConfigurator
{
    public static IServiceCollection AddCommands(this IServiceCollection serviceCollection, QueueBotSettings queueBotSettings, CommandsSettings commandsSettings)
    {
        serviceCollection.AddTransient<QueueBotSettings>(x => queueBotSettings);
        serviceCollection.AddTransient<CommandsSettings>(x => commandsSettings);

        serviceCollection.AddScoped<ICommandExecutor, SwitchNotificationCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, SetTimetableCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, ShowTimetableCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, ShowSubjectsCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, ShowQueueCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, ShowWaitingCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, AddSubjectCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, JoinCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, QuitCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, SkipCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, UnionQueueCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, RenameCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, SetGroupCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, StartCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, StopCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, HelpCommandExecutor>(x => new HelpCommandExecutor(
            x.GetServices<ICommandExecutor>,
            x.GetRequiredService<QueueBotSettings>(),
            x.GetRequiredService<CommandsSettings>()));
        
        serviceCollection.AddScoped<ICommandProvider, CommandProvider>();

        return serviceCollection;
    }
}