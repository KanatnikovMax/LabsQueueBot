using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.BusinessLogic.Services.Implementation;
using LabsQueueBot.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace LabsQueueBot.BusinessLogic;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddManagementServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUserManagementService, UserManagementService>();
        serviceCollection.AddScoped<ISubjectsManagementService, SubjectsManagementService>();
        serviceCollection.AddScoped<IRandomizeUnionWaitingService, RandomizeUnionWaitingService>();
        // serviceCollection.AddSingleton<IUserCleanerService>(x => new UserCleanerService(
        //     x.GetRequiredService<ITelegramBotClient>(),
        //     x.GetRequiredService<IServiceScopeFactory>(),
        //     x.GetRequiredService<IOptions<TelegramBotSettings>>()));
        serviceCollection.AddSingleton<IUserCleanerService, UserCleanerService>();

        return serviceCollection;
    }
}