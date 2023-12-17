using Extensions.Options.EntityFrameworkCore.Watcher;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace Extensions.Options.EntityFrameworkCore.SourceBuilder;

internal class EntityFrameworkCoreConfigurationSourceBuilder<TConfigEntity> : IEntityFrameworkCoreConfigurationSourceBuilder<TConfigEntity>
    where TConfigEntity : class, IConfigEntity
{
    public Func<DbContext>? GetContextFunc { get; private set; }
    public Expression<Func<TConfigEntity, bool>>? Filter { get; private set; }
    public TimeSpan? PeriodicalRefreshTimeSpan { get; private set; }


    public IEntityFrameworkCoreConfigurationSourceBuilder<TConfigEntity> UseGetDbContextFunc(Func<DbContext> getDbContextFunc)
    {
        GetContextFunc = getDbContextFunc;
        return this;
    }

    public IEntityFrameworkCoreConfigurationSourceBuilder<TConfigEntity> UseQueryFilter(Expression<Func<TConfigEntity, bool>>? predicate)
    {
        Filter = predicate;
        return this;
    }

    public IEntityFrameworkCoreConfigurationSourceBuilder<TConfigEntity> EnablePeriodicalAutoRefresh(TimeSpan refreshInterval)
    {
        if (refreshInterval < TimeSpan.Zero)
            throw new Exception($"Refresh interval must be positive.");

        PeriodicalRefreshTimeSpan = refreshInterval;
        return this;
    }



    public EntityFrameworkCoreConfigurationSource<TConfigEntity> Build()
    {
        if (GetContextFunc == null)
        {
            throw new ArgumentNullException(nameof(GetContextFunc), "Get Context delegate is null");
        }

        var source = new EntityFrameworkCoreConfigurationSource<TConfigEntity>()
        {
            GetContextFunc = GetContextFunc,
            Filter = Filter
        };

        if (PeriodicalRefreshTimeSpan != null)
            source.Watcher = new EntityFrameworkCorePeriodicalWatcher(PeriodicalRefreshTimeSpan.Value);

        return source;
    }


}
