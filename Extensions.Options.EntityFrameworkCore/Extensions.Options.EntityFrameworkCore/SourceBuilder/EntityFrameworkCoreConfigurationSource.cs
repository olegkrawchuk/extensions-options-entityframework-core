using Extensions.Options.EntityFrameworkCore.Watcher;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq.Expressions;

namespace Extensions.Options.EntityFrameworkCore.SourceBuilder;

public class EntityFrameworkCoreConfigurationSource<TConfigEntity> : IConfigurationSource where TConfigEntity : class, IConfigEntity
{
    /// <summary>
    /// Делегат, яким ми отримаємо DbContext 
    /// </summary>
    public required Func<DbContext> GetContextFunc { get; init; }

    public required Expression<Func<TConfigEntity, bool>>? Filter { get; init; }

    internal IEntityFrameworkCoreWatcher? Watcher { get; set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new EntityFrameworkCoreConfigurationProvider<TConfigEntity>(this);
    }
}
