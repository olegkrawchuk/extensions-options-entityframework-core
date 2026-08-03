using Extensions.Options.EntityFrameworkCore.Diagnostics;
using Extensions.Options.EntityFrameworkCore.SourceBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Extensions.Options.EntityFrameworkCore.Refresh;

internal sealed class EntityFrameworkCoreConfigurationRefreshService<TDbContext, TConfigEntity> : BackgroundService
    where TConfigEntity : class, IConfigEntity
    where TDbContext : DbContext
{
    private readonly EntityFrameworkCoreConfigurationSource<TDbContext, TConfigEntity> _source;
    private readonly ILogger<EntityFrameworkCoreConfigurationRefreshService<TDbContext, TConfigEntity>> _logger;
    private readonly TimeProvider _timeProvider;

    public EntityFrameworkCoreConfigurationRefreshService(
        EntityFrameworkCoreConfigurationSource<TDbContext, TConfigEntity> source,
        ILogger<EntityFrameworkCoreConfigurationRefreshService<TDbContext, TConfigEntity>> logger,
        TimeProvider? timeProvider = null)
    {
        _source = source;
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var provider = _source.Provider ?? throw new InvalidOperationException(
            "EntityFrameworkCoreConfigurationProvider was not created yet. AddEntityFrameworkConfiguration must add the " +
            "configuration source to the IConfigurationBuilder before the host starts.");

        var changeSignal = _source.ChangeSignal ?? throw new InvalidOperationException(
            "Periodical refresh was requested but no change signal was created for this configuration source.");

        var interval = _source.Options.PeriodicalRefreshInterval!.Value;

        // This is where the final DI container's ILogger reaches the provider — database access
        // itself never depends on it, only diagnostics do.
        provider.AttachDiagnosticsLogger(_logger);
        EntityFrameworkCoreConfigurationLog.PeriodicalRefreshStarted(_logger, interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await DelayAsync(_timeProvider, interval, stoppingToken).ConfigureAwait(false);

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            // Signal() cancels the change-token; the already-registered ChangeToken.OnChange in
            // Provider synchronously invokes Load() (with its own retry) as a result. No separate
            // reload call is made here — that would duplicate the database query on every tick.
            changeSignal.Signal();
        }
    }

    private static async Task DelayAsync(TimeProvider timeProvider, TimeSpan delay, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var timer = timeProvider.CreateTimer(static s => ((TaskCompletionSource)s!).TrySetResult(), tcs, delay, Timeout.InfiniteTimeSpan);
        using var registration = cancellationToken.Register(static s => ((TaskCompletionSource)s!).TrySetCanceled(), tcs);
        await tcs.Task.ConfigureAwait(false);
    }
}
