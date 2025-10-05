using LabsQueueBot.Core.Enums;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.DataAccess;
using LabsQueueBot.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LabsQueueBot.Web.ServiceCollectionExtensions;

public static class UserRepositoryConfigurator
{
    public static async Task<IServiceProvider> InitializeRepository(this IServiceProvider services, CancellationToken cancellationToken)
    {
        var options = services.GetRequiredService<IOptions<TelegramBotSettings>>();
        
        using var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<QueueBotContext>>();

        await InternalInitializeRepository(dbContextFactory, options.Value, cancellationToken);

        return services;
    }
    
    private static async Task InternalInitializeRepository(IDbContextFactory<QueueBotContext> contextFactory,
        TelegramBotSettings settings, CancellationToken cancellationToken)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);

        await CreateFunction_update_StateModifiedAt(dbContext, cancellationToken);
        await CreateTrigger_trigg_update_StateModifiedAt_on_UPDATE(dbContext, cancellationToken);
        
        foreach (var id in settings.PrivilegedChatId)
        {
            var user = await dbContext.UserRepository.Where(ur => ur.Id == id).FirstOrDefaultAsync(cancellationToken);
            
            if (user != null && user.Role == Role.Privileged)
            {
                continue;
            }

            if (user != null && user.Role != Role.Privileged)
            {
                user.Role = Role.Privileged;
                dbContext.UserRepository.Update(user);
                
                continue;
            }
            
            user = new User(id)
            {
                Role = Role.Privileged
            };
            await dbContext.UserRepository.AddAsync(user, cancellationToken);
        }

        foreach (var id in settings.AdminChatId)
        {
            var user = await dbContext.UserRepository.Where(ur => ur.Id == id).FirstOrDefaultAsync(cancellationToken);
            
            if (user is not null && user.Role == Role.Admin)
            {
                continue;
            }
            
            if (user is not null && user.Role != Role.Admin)
            {
                user.Role = Role.Admin;
                dbContext.UserRepository.Update(user);
                
                continue;
            }
            
            user = new User(id)
            {
                Role = Role.Admin
            };
            await dbContext.UserRepository.AddAsync(user, cancellationToken);
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task CreateFunction_update_StateModifiedAt(DbContext dbContext, CancellationToken cancellationToken)
    {// TODO подумать о проверке перед обновлением времени активности
        var query = @"
CREATE OR REPLACE FUNCTION update_LastActivityAt()
RETURNS TRIGGER AS $$ 
BEGIN
    NEW.""LastActivityAt"" = TIMEZONE('utc', NOW());
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
";
        
        await dbContext.Database.ExecuteSqlRawAsync(query, cancellationToken);
    }
    
    private static async Task CreateTrigger_trigg_update_StateModifiedAt_on_UPDATE(DbContext dbContext, CancellationToken cancellationToken)
    {
        var query = @"
DROP TRIGGER IF EXISTS trigg_update_LastActivityAt_on_UPDATE ON public.""UserRepository"";

CREATE TRIGGER trigg_update_LastActivityAt_on_UPDATE
BEFORE INSERT OR UPDATE ON public.""UserRepository""
FOR EACH ROW
EXECUTE FUNCTION update_LastActivityAt();
";
        
        await dbContext.Database.ExecuteSqlRawAsync(query, cancellationToken);
    }
}