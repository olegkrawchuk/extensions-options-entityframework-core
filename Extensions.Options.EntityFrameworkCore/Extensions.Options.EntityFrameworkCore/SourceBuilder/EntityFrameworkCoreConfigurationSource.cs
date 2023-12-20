using Extensions.Options.EntityFrameworkCore.Watcher;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq.Expressions;

namespace Extensions.Options.EntityFrameworkCore.SourceBuilder;

public class EntityFrameworkCoreConfigurationSource<TDbContext, TConfigEntity> : IConfigurationSource
    where TConfigEntity : class, IConfigEntity
    where TDbContext : DbContext
{
    public required IServiceProvider ServiceProvider { get; init; }

    public required Expression<Func<TConfigEntity, bool>>? Filter { get; init; }

    internal IEntityFrameworkCoreWatcher? Watcher { get; set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new EntityFrameworkCoreConfigurationProvider<TDbContext, TConfigEntity>(this);
    }
}
