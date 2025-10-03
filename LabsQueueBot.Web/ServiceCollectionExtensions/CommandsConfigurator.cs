using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.Commands;
using LabsQueueBot.Web.Commands.Implements;
using LabsQueueBot.Web.Providers;
using LabsQueueBot.Web.Providers.Services;
using Microsoft.Extensions.Options;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class CommandsConfigurator
{
    public static IServiceCollection AddCommandExecutors(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddScoped<ICommandExecutor, SwitchNotificationCommandExecutor>();
        // serviceCollection.AddScoped<ICommandExecutor, SetTimetableCommandExecutor>();
        // serviceCollection.AddScoped<ICommandExecutor, ShowTimetableCommandExecutor>();
        // TODO GrantCommandExecutor
        // TODO RevokeCommandExecutor
        serviceCollection.AddScoped<ICommandExecutor, ShowSubjectsCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, ShowQueueCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, AddSubjectCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, JoinCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, QuitCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, SkipCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, UnionQueueCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, RenameCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, SetGroupCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, StartCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, StopCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, BanCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, UnbanCommandExecutor>();
        serviceCollection.AddScoped<ICommandExecutor, HelpCommandExecutor>(x => new HelpCommandExecutor(
            x.GetServices<ICommandExecutor>,
            x.GetRequiredService<IOptions<TelegramBotSettings>>(), 
            x.GetRequiredService<IOptions<CommandsSettings>>(), 
            x.GetRequiredService<ILogger>()));
        
        serviceCollection.AddScoped<ICommandExecutorProvider, CommandExecutorProvider>();

        return serviceCollection;
    }
}