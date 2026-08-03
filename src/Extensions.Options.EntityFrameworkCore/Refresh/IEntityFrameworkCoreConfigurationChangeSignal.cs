using Microsoft.Extensions.Primitives;
using System;

namespace Extensions.Options.EntityFrameworkCore.Refresh;

// Internal wiring between EntityFrameworkCoreConfigurationSource, EntityFrameworkCoreConfigurationProvider
// and EntityFrameworkCoreConfigurationRefreshService. Not part of the public API — consumers never
// implement or call this directly.
internal interface IEntityFrameworkCoreConfigurationChangeSignal : IDisposable
{
    IChangeToken Watch();

    void Signal();
}
