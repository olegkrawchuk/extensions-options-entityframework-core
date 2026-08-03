using Extensions.Options.EntityFrameworkCore.Diagnostics;
using Extensions.Options.EntityFrameworkCore.Retry;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace Extensions.Options.EntityFrameworkCore.Options;

/// <summary>
/// Configures the EF Core configuration source, passed to
/// <see cref="EntityFrameworkCoreConfigurationBuilderExtensions.AddEntityFrameworkConfiguration{TDbContext, TConfigEntity}"/>.
/// </summary>
public class EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity>
    where TConfigEntity : class, IConfigEntity
    where TDbContext : DbContext
{
    /// <summary>
    /// Required. Configures the <see cref="DbContextOptionsBuilder{TContext}"/> — connection
    /// string and database provider (e.g. <c>o =&gt; o.UseNpgsql(connectionString)</c>). Invoked
    /// separately for the initial read and for every periodic refresh — no side effects should be
    /// assumed to carry over between invocations.
    /// </summary>
    public Action<DbContextOptionsBuilder<TDbContext>>? ConfigureDbContext { get; set; }

    /// <summary>
    /// Optional custom <typeparamref name="TDbContext"/> factory — an escape hatch for types
    /// without the standard <c>TDbContext(DbContextOptions&lt;TDbContext&gt;)</c> constructor. If
    /// not set, <see cref="Activator.CreateInstance(Type, object[])"/> is used with that constructor.
    /// </summary>
    public Func<DbContextOptions<TDbContext>, TDbContext>? DbContextFactory { get; set; }

    /// <summary>Optional LINQ predicate to filter table rows. Without it, all rows are read.</summary>
    public Expression<Func<TConfigEntity, bool>>? Filter { get; set; }

    /// <summary>
    /// The periodic refresh interval. If not set, configuration is read only once, at application startup.
    /// </summary>
    public TimeSpan? PeriodicalRefreshInterval { get; set; }

    /// <summary>Configures the library's own <see cref="Microsoft.Extensions.Logging.ILogger"/>-based diagnostics.</summary>
    public EntityFrameworkCoreConfigurationDiagnosticsOptions Diagnostics { get; set; } = new();

    /// <summary>The retry policy applied when the database is unavailable.</summary>
    public EntityFrameworkCoreConfigurationRetryOptions Retry { get; set; } = new();
}
