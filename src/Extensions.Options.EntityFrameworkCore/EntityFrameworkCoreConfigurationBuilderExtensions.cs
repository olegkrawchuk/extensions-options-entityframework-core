using Extensions.Options.EntityFrameworkCore.Options;
using Extensions.Options.EntityFrameworkCore.Refresh;
using Extensions.Options.EntityFrameworkCore.SourceBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Extensions.Options.EntityFrameworkCore;

/// <summary>Extension methods for adding an EF Core-backed configuration source to an <see cref="IConfigurationBuilder"/>.</summary>
public static class EntityFrameworkCoreConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds a configuration source that reads key/value pairs from a database table through
    /// <typeparamref name="TDbContext"/>. Database access is fully self-contained — the library
    /// builds its own <see cref="DbContextOptionsBuilder{TContext}"/> from
    /// <see cref="EntityFrameworkConfigurationOptions{TDbContext, TConfigEntity}.ConfigureDbContext"/>
    /// and never resolves <typeparamref name="TDbContext"/> from the application's DI container.
    /// </summary>
    /// <typeparam name="TDbContext">
    /// An EF Core <see cref="DbContext"/> with the standard <c>TDbContext(DbContextOptions&lt;TDbContext&gt;)</c>
    /// constructor (the same requirement <c>AddDbContext&lt;T&gt;</c> makes), unless
    /// <see cref="EntityFrameworkConfigurationOptions{TDbContext, TConfigEntity}.DbContextFactory"/> is set.
    /// </typeparam>
    /// <typeparam name="TConfigEntity">The configuration table entity, implementing <see cref="IConfigEntity"/>.</typeparam>
    /// <param name="builder">The configuration builder to add the source to.</param>
    /// <param name="services">
    /// Only used when
    /// <see cref="EntityFrameworkConfigurationOptions{TDbContext, TConfigEntity}.PeriodicalRefreshInterval"/>
    /// is set — to register the background refresh service that starts once the host has been built.
    /// </param>
    /// <param name="configure">Configures the configuration source.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="EntityFrameworkConfigurationOptions{TDbContext, TConfigEntity}.ConfigureDbContext"/> is not set.
    /// </exception>
    public static IConfigurationBuilder AddEntityFrameworkConfiguration<TDbContext, TConfigEntity>(
        this IConfigurationBuilder builder,
        IServiceCollection services,
        Action<EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity>> configure)
        where TConfigEntity : class, IConfigEntity
        where TDbContext : DbContext
    {
        var options = new EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity>();
        configure(options);

        if (options.ConfigureDbContext is null)
        {
            throw new ArgumentException(
                $"{nameof(EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity>.ConfigureDbContext)} must be set.",
                nameof(configure));
        }

        var source = new EntityFrameworkCoreConfigurationSource<TDbContext, TConfigEntity>(options);
        builder.Add(source);

        if (options.PeriodicalRefreshInterval.HasValue)
        {
            services.AddSingleton(source);
            services.AddHostedService<EntityFrameworkCoreConfigurationRefreshService<TDbContext, TConfigEntity>>();
        }

        return builder;
    }
}
