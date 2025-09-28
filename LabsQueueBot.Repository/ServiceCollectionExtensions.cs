using LabsQueueBot.Repository.Repository;
using LabsQueueBot.Repository.Repository.Implements;
using Microsoft.Extensions.DependencyInjection;

namespace LabsQueueBot.Repository;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUserRepository, UserRepository>();
        serviceCollection.AddScoped<ISubjectRepository, SubjectRepository>();
        serviceCollection.AddScoped<IBlackListRepository, BlackListRepository>();

        return serviceCollection;
    }
}