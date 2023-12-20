using Extensions.Options.EntityFrameworkCore.SourceBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace Extensions.Options.EntityFrameworkCore;

public class EntityFrameworkCoreConfigurationProvider<TDbContext, TConfigEntity> : ConfigurationProvider, IDisposable
    where TConfigEntity : class, IConfigEntity
    where TDbContext : DbContext
{
    private readonly EntityFrameworkCoreConfigurationSource<TDbContext, TConfigEntity> _source;
    private readonly IDisposable? _changeTokenRegistration;

    public EntityFrameworkCoreConfigurationProvider(EntityFrameworkCoreConfigurationSource<TDbContext, TConfigEntity> source)
    {
        _source = source;

        if (_source.Watcher != null)
        {
            _changeTokenRegistration = ChangeToken.OnChange(
                () => _source.Watcher.Watch(),
                Load
            );
        }
    }

    public override void Load()
    {
        using var scope = _source.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var query = context.Set<TConfigEntity>().AsNoTracking();

        if (_source.Filter is { } filter)
        {
            query = query.Where(filter);
        }

        try
        {
            Data = query.Select(p => new { p.Name, p.Value }).ToDictionary(e => e.Name, e => e.Value);
        }
        catch
        {
            // ignore
        }

        base.Load();
    }


    public void Dispose()
    {
        _changeTokenRegistration?.Dispose();
        _source.Watcher?.Dispose();
        GC.SuppressFinalize(this);
    }
}
