using Microsoft.Extensions.Primitives;
using System;
using System.Threading;

namespace Extensions.Options.EntityFrameworkCore.Refresh;

internal sealed class EntityFrameworkCoreConfigurationChangeSignal : IEntityFrameworkCoreConfigurationChangeSignal
{
    private readonly object _lock = new();
    private CancellationTokenSource? _cancellationTokenSource;

    public IChangeToken Watch()
    {
        lock (_lock)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            return new CancellationChangeToken(_cancellationTokenSource.Token);
        }
    }

    public void Signal()
    {
        CancellationTokenSource? previous;

        lock (_lock)
        {
            previous = _cancellationTokenSource;
        }

        previous?.Cancel();
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _cancellationTokenSource?.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
