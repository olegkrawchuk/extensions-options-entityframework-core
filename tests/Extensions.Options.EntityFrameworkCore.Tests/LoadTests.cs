using Extensions.Options.EntityFrameworkCore.Options;
using Extensions.Options.EntityFrameworkCore.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace Extensions.Options.EntityFrameworkCore.Tests;

public class LoadTests
{
    [Test]
    public async Task Load_ReadsAllSeededRows_WhenNoFilterApplied()
    {
        var databaseName = Guid.NewGuid().ToString();
        SeedData.Seed(databaseName);

        var options = new EntityFrameworkConfigurationOptions<TestDbContext, TestConfigEntity>
        {
            ConfigureDbContext = o => o.UseInMemoryDatabase(databaseName),
        };

        var provider = new EntityFrameworkCoreConfigurationProvider<TestDbContext, TestConfigEntity>(options, null);
        provider.Load();

        foreach (var (name, value) in SeedData.Entries)
        {
            provider.TryGet(name, out var actual);
            await Assert.That(actual).IsEqualTo(value);
        }
    }

    [Test]
    public async Task Load_ExcludesFilteredRows_WhenFilterApplied()
    {
        var databaseName = Guid.NewGuid().ToString();
        SeedData.Seed(databaseName);

        var options = new EntityFrameworkConfigurationOptions<TestDbContext, TestConfigEntity>
        {
            ConfigureDbContext = o => o.UseInMemoryDatabase(databaseName),
            Filter = entity => entity.Value != null,
        };

        var provider = new EntityFrameworkCoreConfigurationProvider<TestDbContext, TestConfigEntity>(options, null);
        provider.Load();

        foreach (var (name, value) in SeedData.EntriesWithValue)
        {
            provider.TryGet(name, out var actual);
            await Assert.That(actual).IsEqualTo(value);
        }

        var excluded = SeedData.Entries.Except(SeedData.EntriesWithValue).Single();
        var found = provider.TryGet(excluded.Name, out _);
        await Assert.That(found).IsFalse();
    }

    [Test]
    public async Task Load_KeepsPreviousDataAndRecordsException_WhenReadingFailsAfterAllRetries()
    {
        var options = new EntityFrameworkConfigurationOptions<TestDbContext, TestConfigEntity>
        {
            ConfigureDbContext = o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()),
            Retry = { MaxAttempts = 2, InitialDelay = TimeSpan.Zero, MaxDelay = TimeSpan.Zero },
        };

        var provider = new AlwaysFailingProvider(options);
        provider.Load();

        await Assert.That(provider.LastLoadException).IsNotNull();
        await Assert.That(provider.Attempts).IsEqualTo(2);

        var found = provider.TryGet("anything", out _);
        await Assert.That(found).IsFalse();
    }

    [Test]
    public async Task Load_Succeeds_WhenReadingFailsFewerTimesThanMaxAttempts()
    {
        var databaseName = Guid.NewGuid().ToString();
        SeedData.Seed(databaseName);

        var options = new EntityFrameworkConfigurationOptions<TestDbContext, TestConfigEntity>
        {
            ConfigureDbContext = o => o.UseInMemoryDatabase(databaseName),
            Retry = { MaxAttempts = 3, InitialDelay = TimeSpan.Zero, MaxDelay = TimeSpan.Zero },
        };

        var provider = new FailTwiceThenSucceedProvider(options);
        provider.Load();

        await Assert.That(provider.LastLoadException).IsNull();
        await Assert.That(provider.Attempts).IsEqualTo(3);

        var found = provider.TryGet(SeedData.Entries[0].Name, out var value);
        await Assert.That(found).IsTrue();
        await Assert.That(value).IsEqualTo(SeedData.Entries[0].Value);
    }

    [Test]
    public async Task Load_Throws_WhenDbContextHasNoStandardConstructor()
    {
        var options = new EntityFrameworkConfigurationOptions<NonStandardDbContext, TestConfigEntity>
        {
            ConfigureDbContext = o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()),
        };

        var provider = new EntityFrameworkCoreConfigurationProvider<NonStandardDbContext, TestConfigEntity>(options, null);
        provider.Load();

        await Assert.That(provider.LastLoadException).IsNotNull();
        await Assert.That(provider.LastLoadException).IsTypeOf<InvalidOperationException>();
    }

    private sealed class AlwaysFailingProvider : EntityFrameworkCoreConfigurationProvider<TestDbContext, TestConfigEntity>
    {
        public AlwaysFailingProvider(EntityFrameworkConfigurationOptions<TestDbContext, TestConfigEntity> options)
            : base(options, null)
        {
        }

        public int Attempts { get; private set; }

        protected override IDictionary<string, string?> ReadEntries()
        {
            Attempts++;
            throw new InvalidOperationException("Simulated failure");
        }
    }

    private sealed class FailTwiceThenSucceedProvider : EntityFrameworkCoreConfigurationProvider<TestDbContext, TestConfigEntity>
    {
        public FailTwiceThenSucceedProvider(EntityFrameworkConfigurationOptions<TestDbContext, TestConfigEntity> options)
            : base(options, null)
        {
        }

        public int Attempts { get; private set; }

        protected override IDictionary<string, string?> ReadEntries()
        {
            Attempts++;
            return Attempts <= 2
                ? throw new InvalidOperationException("Simulated failure")
                : base.ReadEntries();
        }
    }

    private sealed class NonStandardDbContext : DbContext
    {
        public NonStandardDbContext(DbContextOptions<NonStandardDbContext> options, string extraParameter) : base(options)
        {
            _ = extraParameter;
        }
    }
}
