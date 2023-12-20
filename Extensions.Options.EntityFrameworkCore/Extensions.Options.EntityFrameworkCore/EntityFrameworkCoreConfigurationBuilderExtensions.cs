using Extensions.Options.EntityFrameworkCore.SourceBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq.Expressions;

namespace Extensions.Options.EntityFrameworkCore;

public static class EntityFrameworkCoreConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddEntityFramework<TDbContext, TConfigEntity>(this IConfigurationBuilder builder,
        Action<IEntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity>> builderAction)
        where TConfigEntity : class, IConfigEntity
        where TDbContext : DbContext
    {
        var sqlBuilder = new EntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity>();
        builderAction(sqlBuilder);

        var source = sqlBuilder.Build();
        return builder.Add(source);
    }
}
