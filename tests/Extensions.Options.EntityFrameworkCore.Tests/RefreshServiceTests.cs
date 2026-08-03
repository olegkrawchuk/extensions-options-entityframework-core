using Extensions.Options.EntityFrameworkCore.Options;
using Extensions.Options.EntityFrameworkCore.Refresh;
using Extensions.Options.EntityFrameworkCore.SourceBuilder;
using Extensions.Options.EntityFrameworkCore.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace Extensions.Options.EntityFrameworkCore.Tests;

public class RefreshServiceTests
{
    [Test]
    public async Task ExecuteAsync_LogsStartupOnce_AndTriggersReloadOnEachInterval()
    {
        var databaseName = Guid.NewGuid().ToString();
        SeedData.Seed(databaseName);

        var interval = TimeSpan.FromSeconds(30);
        var options = new EntityFrameworkConfigurationOptions<TestDbContext, TestConfigEntity>
        {
            ConfigureDbContext = o => o.UseInMemoryDatabase(databaseName),
            PeriodicalRefreshInterval = interval,
        };

        var source = new EntityFrameworkCoreConfigurationSource<TestDbContext, TestConfigEntity>(options);
        var configurationRoot = new ConfigurationBuilder().Add(source).Build();

        var timeProvider = new FakeTimeProvider();
        var logger = new RecordingLogger<EntityFrameworkCoreConfigurationRefreshService<TestDbContext, TestConfigEntity>>();
        var refreshService = new EntityFrameworkCoreConfigurationRefreshService<TestDbContext, TestConfigEntity>(source, logger, timeProvider);

        using var cts = new CancellationTokenSource();
        await refreshService.StartAsync(cts.Token);

        await Task.Delay(100);
        await Assert.That(logger.Messages.Any(m => m.Contains("Periodical configuration refresh started"))).IsTrue();

        using (var seedContext = new TestDbContext(SeedData.CreateInMemoryOptions(databaseName)))
        {
            seedContext.ConfigEntries.Add(new TestConfigEntity { Name = "New:Key", Value = "new-value" });
            seedContext.SaveChanges();
        }

        await Assert.That(configurationRoot["New:Key"]).IsNull();

        timeProvider.Advance(interval);
        await Task.Delay(100);

        await Assert.That(configurationRoot["New:Key"]).IsEqualTo("new-value");

        await refreshService.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task ExecuteAsync_DoesNotSignal_BeforeIntervalElapses()
    {
        var databaseName = Guid.NewGuid().ToString();
        SeedData.Seed(databaseName);

        var interval = TimeSpan.FromMinutes(5);
        var options = new EntityFrameworkConfigurationOptions<TestDbContext, TestConfigEntity>
        {
            ConfigureDbContext = o => o.UseInMemoryDatabase(databaseName),
            PeriodicalRefreshInterval = interval,
        };

        var source = new EntityFrameworkCoreConfigurationSource<TestDbContext, TestConfigEntity>(options);
        var configurationRoot = new ConfigurationBuilder().Add(source).Build();

        var timeProvider = new FakeTimeProvider();
        var logger = new RecordingLogger<EntityFrameworkCoreConfigurationRefreshService<TestDbContext, TestConfigEntity>>();
        var refreshService = new EntityFrameworkCoreConfigurationRefreshService<TestDbContext, TestConfigEntity>(source, logger, timeProvider);

        using var cts = new CancellationTokenSource();
        await refreshService.StartAsync(cts.Token);
        await Task.Delay(50);

        using (var seedContext = new TestDbContext(SeedData.CreateInMemoryOptions(databaseName)))
        {
            seedContext.ConfigEntries.Add(new TestConfigEntity { Name = "New:Key", Value = "new-value" });
            seedContext.SaveChanges();
        }

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(50);

        await Assert.That(configurationRoot["New:Key"]).IsNull();

        await refreshService.StopAsync(CancellationToken.None);
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
