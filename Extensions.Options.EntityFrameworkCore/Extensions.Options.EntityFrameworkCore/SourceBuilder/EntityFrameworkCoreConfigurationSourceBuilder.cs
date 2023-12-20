using Extensions.Options.EntityFrameworkCore.Watcher;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace Extensions.Options.EntityFrameworkCore.SourceBuilder;

internal class EntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity>
    : IEntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity>
    where TConfigEntity : class, IConfigEntity
    where TDbContext : DbContext
{
    public IServiceProvider? ServiceProvider { get; private set; }
    public Expression<Func<TConfigEntity, bool>>? Filter { get; private set; }
    public TimeSpan? PeriodicalRefreshTimeSpan { get; private set; }


    public IEntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity> UseServiceProvider(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        return this;
    }

    public IEntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity> UseQueryFilter(Expression<Func<TConfigEntity, bool>>? predicate)
    {
        Filter = predicate;
        return this;
    }

    public IEntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity> EnablePeriodicalAutoRefresh(TimeSpan refreshInterval)
    {
        if (refreshInterval < TimeSpan.Zero)
            throw new Exception($"Refresh interval must be positive.");

        PeriodicalRefreshTimeSpan = refreshInterval;
        return this;
    }



    public EntityFrameworkCoreConfigurationSource<TDbContext, TConfigEntity> Build()
    {
        if (ServiceProvider == null)
        {
            throw new ArgumentNullException(nameof(ServiceProvider), "ServiceProvider is null");
        }

        var source = new EntityFrameworkCoreConfigurationSource<TDbContext, TConfigEntity>()
        {
            ServiceProvider = ServiceProvider,
            Filter = Filter
        };

        if (PeriodicalRefreshTimeSpan != null)
            source.Watcher = new EntityFrameworkCorePeriodicalWatcher(PeriodicalRefreshTimeSpan.Value);

        return source;
    }


}
