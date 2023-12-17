using Extensions.Options.EntityFrameworkCore.SourceBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace Extensions.Options.EntityFrameworkCore;

public class EntityFrameworkCoreConfigurationProvider<TConfigEntity> : ConfigurationProvider, IDisposable where TConfigEntity : class, IConfigEntity
{
    private readonly EntityFrameworkCoreConfigurationSource<TConfigEntity> _source;
    private readonly IDisposable? _changeTokenRegistration;

    public EntityFrameworkCoreConfigurationProvider(EntityFrameworkCoreConfigurationSource<TConfigEntity> source)
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
        using var context = _source.GetContextFunc();

        var query = context.Set<TConfigEntity>().AsNoTracking();

        if (_source.Filter is { } filter)
        {
            query = query.Where(filter);
        }

        try
        {
            Data = query.ToDictionary(e => e.Name, e => e.Value);
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
