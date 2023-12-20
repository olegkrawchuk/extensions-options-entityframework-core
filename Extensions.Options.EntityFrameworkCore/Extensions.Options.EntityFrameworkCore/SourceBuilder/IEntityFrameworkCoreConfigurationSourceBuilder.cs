using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace Extensions.Options.EntityFrameworkCore.SourceBuilder;

public interface IEntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity>
    where TConfigEntity : class, IConfigEntity
    where TDbContext : DbContext
{
    // TODO: придумати інший спосіб отримати DbContext
    IEntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity> UseServiceProvider(IServiceProvider serviceProvider);

    IEntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity> UseQueryFilter(Expression<Func<TConfigEntity, bool>>? predicate);

    IEntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity> EnablePeriodicalAutoRefresh(TimeSpan refreshInterval);

    EntityFrameworkCoreConfigurationSource<TDbContext, TConfigEntity> Build();
}
