# Extensions.Options.EntityFrameworkCore

A `Microsoft.Extensions.Configuration` provider that reads key/value pairs from a database table
through a regular EF Core `DbContext` — no need to write your own `IConfigurationProvider` from
scratch.

## Features

| Aspect | Value |
|---|---|
| Data source | any table, through your own EF Core `DbContext` |
| Database | any database with an EF Core provider (PostgreSQL, SQL Server, SQLite, Oracle, etc.) — chosen by the consumer, the library isn't tied to a specific database |
| Periodic refresh | optional, via `options.PeriodicalRefreshInterval` |
| Resilience to database outages | retry with exponential backoff (`options.Retry`), without crashing the application |
| EF Core SQL logging | disabled by default |
| Diagnostics | opt-in, through `ILogger` — silent unless explicitly requested |
| Row filtering | any LINQ predicate (`options.Filter`) |

> [!NOTE]
> The library never resolves `TDbContext` from the application's DI container. Instead, it builds
> its own `DbContextOptionsBuilder<TDbContext>` from `options.ConfigureDbContext`. This removes any
> dependency on the order services are registered in — database access behaves the same way for the
> initial read and for every periodic refresh.

## Installation

```
dotnet add package Extensions.Options.EntityFrameworkCore
```

Target frameworks: `net8.0`, `net10.0`.

## Quick start

The configuration table entity implements `IConfigEntity`:

```csharp
public class DbOptionEntity : IConfigEntity
{
    public string Name { get; set; } = default!;
    public string? Value { get; set; }
}
```

`TDbContext` is any `DbContext` with the standard `TDbContext(DbContextOptions<TDbContext> options)`
constructor and a `DbSet<DbOptionEntity>` in its model (this can be your application's main
`DbContext` or a separate one dedicated to configuration).

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEntityFrameworkConfiguration<AppDbContext, DbOptionEntity>(builder.Services, options =>
{
    options.ConfigureDbContext = o => o.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
    options.Filter = e => e.Value != null;
    options.PeriodicalRefreshInterval = TimeSpan.FromSeconds(60);
    options.Diagnostics.LogReloadEvents = true; // optional, silent by default
});
```

Values from the table are then available as regular configuration — through `IConfiguration`,
`IOptions<T>`, etc., alongside every other configuration source in the application.

### Options

| Property | Purpose |
|---|---|
| `ConfigureDbContext` (required) | configures `DbContextOptionsBuilder<TDbContext>`: connection string, database provider |
| `DbContextFactory` | escape hatch — a custom `TDbContext` factory, for a non-standard constructor |
| `Filter` | a LINQ predicate (`Expression<Func<TConfigEntity, bool>>`) to select rows |
| `PeriodicalRefreshInterval` | the refresh interval; if not set, configuration is read only once at startup |
| `Retry.MaxAttempts` / `InitialDelay` / `BackoffFactor` / `MaxDelay` | the retry policy applied when the database is unavailable |
| `Diagnostics.LogReloadEvents` | enable structured logging of successful reload events |

> [!TIP]
> If your `TDbContext` constructor takes more than one parameter (for example, a dedicated
> `ILogger<TDbContext>` for the context's own logging), the default instantiation fails with a
> `MissingMethodException`. Supply a factory explicitly:
>
> ```csharp
> options.DbContextFactory = o => new AppDbContext(o, NullLogger<AppDbContext>.Instance);
> ```
>
> There's no need — and no benefit — to pass the application's "real" `ILogger` here: this
> `DbContext` is intentionally isolated from the application's DI container (see above), and SQL
> logging is already disabled by default regardless. `NullLogger<TDbContext>.Instance` is the
> right default for this parameter.

> [!WARNING]
> Version `2.0.0` is an intentional breaking change from `1.0.0`: `UseServiceProvider` was removed,
> `AddEntityFramework` was renamed to `AddEntityFrameworkConfiguration` with a new signature, and
> `netstandard2.0` support was dropped. See [`CHANGELOG.md`](./CHANGELOG.md) for details.
