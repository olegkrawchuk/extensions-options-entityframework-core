# Extensions.Options.EntityFrameworkCore — implementation notes

For features, installation, and usage, see the [repository README](../../README.md). This file
covers how the library is put together internally.

## Flow

```
AddEntityFrameworkConfiguration(...)
  → builds EntityFrameworkConfigurationOptions
  → creates EntityFrameworkCoreConfigurationSource, adds it to IConfigurationBuilder
  → if PeriodicalRefreshInterval is set, registers EntityFrameworkCoreConfigurationRefreshService (IHostedService)

EntityFrameworkCoreConfigurationSource.Build(...)
  → creates EntityFrameworkCoreConfigurationProvider (and a change-signal, if refresh is enabled)

EntityFrameworkCoreConfigurationProvider.Load()
  → builds its own DbContextOptionsBuilder<TDbContext> (SQL logging off), never touches the
    application's DI container
  → reads rows through RetryPolicyExecutor (retry with backoff)
  → on success: assigns Data; on exhausted retries: keeps LastLoadException, leaves Data as is

EntityFrameworkCoreConfigurationRefreshService (BackgroundService)
  → starts once the host has been built — this is where the library's own ILogger (from the final
    DI container) is attached to the provider for diagnostics
  → on each tick: only signals the change token; the actual reload (with retry) runs through the
    already-registered ChangeToken.OnChange callback in the provider — no separate database call
    is made here, to avoid querying twice per tick
```

## Why database access never touches DI

Earlier versions resolved `TDbContext` from an `IServiceProvider` snapshot captured before the host
finished building — a snapshot that never saw the application's real logging pipeline, causing
unformatted EF Core SQL logs to leak past it. The library now builds its own
`DbContextOptionsBuilder<TDbContext>` for every read, so database access behaves identically for
the initial load and for every periodic refresh, regardless of the consumer's own DI setup.
