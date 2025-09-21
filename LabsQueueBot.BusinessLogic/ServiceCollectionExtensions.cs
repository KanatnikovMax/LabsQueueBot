using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.BusinessLogic.Services.Implementation;
using LabsQueueBot.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace LabsQueueBot.BusinessLogic;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserManageServices(this IServiceCollection serviceCollection, QueueBotSettings settings)
    {
        serviceCollection.AddSingleton<IUserCleanerService>(x => new UserCleanerService(
            x.GetRequiredService<ITelegramBotClient>(),
            x.GetRequiredService<IServiceScopeFactory>(),
            settings));

        serviceCollection.AddSingleton<IUserManagementService, UserManagementService>();

        return serviceCollection;
    }
}