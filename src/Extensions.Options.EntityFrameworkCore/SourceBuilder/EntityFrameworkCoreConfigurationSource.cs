using Extensions.Options.EntityFrameworkCore.Options;
using Extensions.Options.EntityFrameworkCore.Refresh;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Extensions.Options.EntityFrameworkCore.SourceBuilder;

internal sealed class EntityFrameworkCoreConfigurationSource<TDbContext, TConfigEntity> : IConfigurationSource
    where TConfigEntity : class, IConfigEntity
    where TDbContext : DbContext
{
    private readonly EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity> _options;

    public EntityFrameworkCoreConfigurationSource(EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity> options)
    {
        _options = options;
    }

    internal EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity> Options => _options;

    internal EntityFrameworkCoreConfigurationProvider<TDbContext, TConfigEntity>? Provider { get; private set; }

    internal IEntityFrameworkCoreConfigurationChangeSignal? ChangeSignal { get; private set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        ChangeSignal = _options.PeriodicalRefreshInterval.HasValue
            ? new EntityFrameworkCoreConfigurationChangeSignal()
            : null;

        Provider = new EntityFrameworkCoreConfigurationProvider<TDbContext, TConfigEntity>(_options, ChangeSignal);

        return Provider;
    }
}
