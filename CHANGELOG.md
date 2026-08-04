# Changelog

Format based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/).

## [2.0.1] - 2026-08-04

### Fixed

- Повідомлення винятку при відсутності відповідного конструктора `TDbContext` тепер містить
  готовий приклад коду для `options.DbContextFactory` замість лише посилання на властивість.

## [2.0.0] - 2026-08-04

### Changed

- **Breaking:** the public API moved to the Options pattern. `AddEntityFramework<TDbContext, TConfigEntity>(Action<IEntityFrameworkCoreConfigurationSourceBuilder<...>>)`
  was replaced with `AddEntityFrameworkConfiguration<TDbContext, TConfigEntity>(IServiceCollection, Action<EntityFrameworkConfigurationOptions<...>>)`.
- **Breaking:** `UseServiceProvider(...)` was removed. The library no longer resolves `TDbContext`
  from an external `IServiceProvider` — neither for the initial configuration read nor for periodic
  refresh. Instead, it uses its own `DbContextOptionsBuilder<TDbContext>`, configured directly by
  the consumer through `options.ConfigureDbContext`. This removes the dependency on a stale DI
  container snapshot captured before the host finished building, which in particular used to leak
  unformatted EF Core SQL logging into the application's own log output.
- **Breaking:** `UseQueryFilter(...)` → `options.Filter`, `EnablePeriodicalAutoRefresh(...)` →
  `options.PeriodicalRefreshInterval`.
- **Breaking:** dropped `netstandard2.0`, `net6.0`, and `net7.0` support. Supported targets: `net8.0`, `net10.0`.
- Periodic configuration refresh now runs through an `IHostedService` that starts after the host
  has been built — this gives the library's own diagnostics access to the final `ILogger` (database
  access itself remains fully self-contained and is unaffected by this).

### Added

- `EntityFrameworkConfigurationOptions<TDbContext, TConfigEntity>.DbContextFactory` — an escape
  hatch for a `TDbContext` without the standard `TDbContext(DbContextOptions<TDbContext>)` constructor.
- Retry with backoff for database outages (`options.Retry`), replacing the previous silent failure.
  Defaults: 3 attempts, 200ms initial delay, ×2 backoff, 5s maximum delay.
- The library's own opt-in diagnostics through `ILogger` (`options.Diagnostics.LogReloadEvents`) —
  silent by default, structured messages only when explicitly requested.

### Removed

- `IEntityFrameworkCoreConfigurationSourceBuilder<TDbContext, TConfigEntity>` and its implementation.
- `IEntityFrameworkCoreWatcher`/`EntityFrameworkCorePeriodicalWatcher` (replaced by an internal
  change-notification mechanism, not part of the public API).
