using Microsoft.Extensions.Primitives;
using System;

namespace Extensions.Options.EntityFrameworkCore.Watcher;

public interface IEntityFrameworkCoreWatcher : IDisposable
{
    IChangeToken Watch();
}
