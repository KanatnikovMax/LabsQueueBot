using LabsQueueBot.Core.Settings;
using LabsQueueBot.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class DbContextConfigurator
{
    public static IServiceCollection AddDbContext(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var settings = configuration.GetRequiredSection(nameof(PostgreSqlSettings)).Get<PostgreSqlSettings>();
        ArgumentNullException.ThrowIfNull(settings);

        var connectionString = settings.ConnectionString;
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

        context.Database.Migrate();

        return services;
    }
}