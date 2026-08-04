using Extensions.Options.EntityFrameworkCore.Diagnostics;
using Extensions.Options.EntityFrameworkCore.Helpers;
using Extensions.Options.EntityFrameworkCore.Options;
using Extensions.Options.EntityFrameworkCore.Refresh;
using Extensions.Options.EntityFrameworkCore.Retry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Extensions.Options.EntityFrameworkCore;

// Not part of the public API — constructed internally by EntityFrameworkCoreConfigurationSource
// and never exposed to consumers directly. Kept accessible to the InternalsVisibleTo-friend test
// assembly for direct construction/subclassing in tests.
internal class EntityFrameworkCoreConfigurationProvider<TDbContext, TConfigEntity> : ConfigurationProvider, IDisposable
    where TConfigEntity : class, IConfigEntity
    where TDbContext : DbContext
{
    private readonly EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity> _options;
    private readonly IEntityFrameworkCoreConfigurationChangeSignal? _changeSignal;
    private readonly IDisposable? _changeTokenRegistration;

    private ILogger _logger = NullLogger.Instance;

    /// <summary>The exception from the most recent failed load attempt, if any, after retries were exhausted.</summary>
    public Exception? LastLoadException { get; private set; }

    internal EntityFrameworkCoreConfigurationProvider(
        EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity> options,
        IEntityFrameworkCoreConfigurationChangeSignal? changeSignal)
    {
        _options = options;
        _changeSignal = changeSignal;

        if (_changeSignal != null)
        {
            _changeTokenRegistration = ChangeToken.OnChange(_changeSignal.Watch, Load);
        }
    }

    // Called by EntityFrameworkCoreConfigurationRefreshService once the host's final DI container
    // exists, so the real ILogger (Serilog, etc.) is used instead of NullLogger — database access
    // never depends on this, only diagnostics do.
    internal void AttachDiagnosticsLogger(ILogger logger)
    {
        _logger = logger;

        if (LastLoadException is { } exception)
        {
            EntityFrameworkCoreConfigurationLog.InitialLoadFailedDeferredNotice(_logger, exception);
        }
    }

    public override void Load()
    {
        using var activity = TracingHelper.NewActivity("LoadConfiguration");

        try
        {
            var entries = RetryPolicyExecutor.Execute(ReadEntries, _options.Retry, _logger);
            Data = entries;
            LastLoadException = null;

            if (_options.Diagnostics.LogReloadEvents)
            {
                EntityFrameworkCoreConfigurationLog.ReloadSucceeded(_logger, entries.Count);
            }
        }
        catch (Exception exception)
        {
            LastLoadException = exception;
        }

        base.Load();
    }

    protected virtual IDictionary<string, string?> ReadEntries()
    {
        using var context = CreateDbContext();

        var query = context.Set<TConfigEntity>().AsNoTracking();

        if (_options.Filter is { } filter)
        {
            query = query.Where(filter);
        }

        return query
            .Select(entity => new { entity.Name, entity.Value })
            .ToDictionary(entity => entity.Name, entity => entity.Value, StringComparer.OrdinalIgnoreCase);
    }

    private TDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();

        // Own DbContextOptionsBuilder, isolated from anything the consumer configured on their own
        // DbContext registration — EF Core SQL command logging is off by default here regardless.
        optionsBuilder.LogTo(_ => { }, LogLevel.None);

        _options.ConfigureDbContext?.Invoke(optionsBuilder);

        var dbContextOptions = optionsBuilder.Options;

        try
        {
            return _options.DbContextFactory != null
                ? _options.DbContextFactory(dbContextOptions)
                : (TDbContext)Activator.CreateInstance(typeof(TDbContext), dbContextOptions)!;
        }
        catch (MissingMethodException exception)
        {
            throw new InvalidOperationException(
                $"{typeof(TDbContext).Name} must expose a public constructor accepting DbContextOptions<{typeof(TDbContext).Name}>, " +
                "the same requirement AddDbContext<T> makes, or a custom factory must be supplied via " +
                $"{nameof(EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity>.DbContextFactory)}, e.g.: " +
                $"options.{nameof(EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity>.DbContextFactory)} = " +
                $"o => new {typeof(TDbContext).Name}(o, NullLogger<{typeof(TDbContext).Name}>.Instance);",
                exception);
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            throw new InvalidOperationException(
                $"{typeof(TDbContext).Name} constructor threw an exception. See inner exception for details.",
                exception.InnerException);
        }
    }

    public void Dispose()
    {
        _changeTokenRegistration?.Dispose();
        _changeSignal?.Dispose();
        GC.SuppressFinalize(this);
    }
}
