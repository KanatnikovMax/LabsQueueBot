using LabsQueueBot.Core.Settings;
using LabsQueueBot.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class DbContextConfigurator
{
    private const string ConnectionStringPattern = "Host={0};Port={1};Database={2};Username={3};Password={4}";

    public static IServiceCollection AddDbContext(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var settings = configuration.GetRequiredSection(nameof(PostgreSqlSettings)).Get<PostgreSqlSettings>();
        ArgumentNullException.ThrowIfNull(settings);
        
        var connectionString = string.Format(ConnectionStringPattern,
            settings.Host, settings.Port, settings.Database, settings.Username, settings.Password);
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