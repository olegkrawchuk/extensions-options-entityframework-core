using Extensions.Options.EntityFrameworkCore.SourceBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq.Expressions;

namespace Extensions.Options.EntityFrameworkCore;

public static class EntityFrameworkCoreConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddEntityFramework<TConfigEntity>(this IConfigurationBuilder builder, Func<DbContext> getDbContextFunc)
        where TConfigEntity : class, IConfigEntity
    {
        return builder.AddEntityFramework<TConfigEntity>(builder => builder.UseGetDbContextFunc(getDbContextFunc));
    }

    public static IConfigurationBuilder AddEntityFramework<TConfigEntity>(this IConfigurationBuilder builder,
        Func<DbContext> getDbContextFunc,
        Expression<Func<TConfigEntity, bool>>? queryFilter,
        TimeSpan periodicalRefreshInterval) where TConfigEntity : class, IConfigEntity
    {
        return builder.AddEntityFramework<TConfigEntity>(builder => builder
            .UseGetDbContextFunc(getDbContextFunc)
            .UseQueryFilter(queryFilter));
    }

    public static IConfigurationBuilder AddEntityFramework<TConfigEntity>(this IConfigurationBuilder builder,
        Action<IEntityFrameworkCoreConfigurationSourceBuilder<TConfigEntity>> builderAction)
        where TConfigEntity : class, IConfigEntity
    {
        var sqlBuilder = new EntityFrameworkCoreConfigurationSourceBuilder<TConfigEntity>();
        builderAction(sqlBuilder);

        var source = sqlBuilder.Build();
        return builder.Add(source);
    }
}
