using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Settings;
using Microsoft.Extensions.Options;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Jobs;

public class UsersCleanerJob(
    IUserStateCleanerService userStateCleanerService,
    IServiceScopeFactory scopeFactory,
    IOptions<CleanerJobSettings> options,
    ILogger logger) : JobBase(logger)
{
    protected override TimeSpan JobDelayBeforeStart => TimeSpan.FromMinutes(options.Value.TimeoutInMinutes);
    protected override TimeSpan JobTimeout => TimeSpan.FromMinutes(options.Value.TimeoutInMinutes);

    protected override async Task JobBody(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var blackListService = scope.ServiceProvider.GetRequiredService<IBlackListManagementService>();
                
        var clearUsers = userStateCleanerService.ClearOrDeleteAll(cancellationToken);
        var unbanUsers = blackListService.UnbanAllByTimeout(cancellationToken);
        Task.WaitAll([clearUsers, unbanUsers], cancellationToken);
    }
}