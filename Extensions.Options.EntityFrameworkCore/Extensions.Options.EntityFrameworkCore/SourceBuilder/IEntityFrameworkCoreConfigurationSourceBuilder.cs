using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace Extensions.Options.EntityFrameworkCore.SourceBuilder;

public interface IEntityFrameworkCoreConfigurationSourceBuilder<TConfigEntity> where TConfigEntity : class, IConfigEntity
{
    // TODO: придумати інший спосіб отримати DbContext
    IEntityFrameworkCoreConfigurationSourceBuilder<TConfigEntity> UseGetDbContextFunc(Func<DbContext> getDbContextFunc);

    IEntityFrameworkCoreConfigurationSourceBuilder<TConfigEntity> UseQueryFilter(Expression<Func<TConfigEntity, bool>>? predicate);

    IEntityFrameworkCoreConfigurationSourceBuilder<TConfigEntity> EnablePeriodicalAutoRefresh(TimeSpan refreshInterval);

    EntityFrameworkCoreConfigurationSource<TConfigEntity> Build();
}
