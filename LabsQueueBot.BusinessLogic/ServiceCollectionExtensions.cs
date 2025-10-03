using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.BusinessLogic.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace LabsQueueBot.BusinessLogic;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddManagementServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUserManagementService, UserManagementService>();
        serviceCollection.AddScoped<ISubjectsManagementService, SubjectsManagementService>();
        serviceCollection.AddScoped<IRandomizeUnionWaitingService, RandomizeUnionWaitingService>();
        serviceCollection.AddScoped<IBlackListManagementService, BlackListManagementService>();
        serviceCollection.AddSingleton<IUserStateCleanerService, UserStateCleanerService>();

        return serviceCollection;
    }
}