using LabsQueueBot.Core.Settings;
using LabsQueueBot.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class DbContextConfigurator
{
    public static IServiceCollection AddDbContext(this IServiceCollection serviceCollection, QueueBotSettings settings)
    {
        var connectionString = settings.QueueBotDbContext;
        serviceCollection.AddDbContextFactory<QueueBotContext>(
            options => { options.UseNpgsql(connectionString); },
            ServiceLifetime.Scoped);

        return serviceCollection;
    }

    public static IServiceProvider ConfigureDbContext(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<QueueBotContext>>();
        using var context = contextFactory.CreateDbContext();

        context.Database.EnsureCreated();
        context.Database.Migrate();

        return services;
    }
}