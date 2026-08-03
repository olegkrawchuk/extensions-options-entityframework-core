using Extensions.Options.EntityFrameworkCore.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Extensions.Options.EntityFrameworkCore.Tests;

public class EndToEndTests
{
    [Test]
    public async Task AddEntityFrameworkConfiguration_ExposesSeededValues_ThroughConfigurationRoot()
    {
        var databaseName = Guid.NewGuid().ToString();
        SeedData.Seed(databaseName);

        var configurationBuilder = new ConfigurationBuilder();
        var services = new ServiceCollection();

        configurationBuilder.AddEntityFrameworkConfiguration<TestDbContext, TestConfigEntity>(services, options =>
        {
            options.ConfigureDbContext = o => o.UseInMemoryDatabase(databaseName);
            options.Filter = entity => entity.Value != null;
        });

        var configurationRoot = configurationBuilder.Build();

        foreach (var (name, value) in SeedData.EntriesWithValue)
        {
            await Assert.That(configurationRoot[name]).IsEqualTo(value);
        }

        var excluded = SeedData.Entries.Except(SeedData.EntriesWithValue).Single();
        await Assert.That(configurationRoot[excluded.Name]).IsNull();
    }

    [Test]
    public async Task AddEntityFrameworkConfiguration_RegistersHostedService_OnlyWhenPeriodicalRefreshEnabled()
    {
        var databaseName = Guid.NewGuid().ToString();
        SeedData.Seed(databaseName);

        var withoutRefreshServices = new ServiceCollection();
        new ConfigurationBuilder().AddEntityFrameworkConfiguration<TestDbContext, TestConfigEntity>(withoutRefreshServices, options =>
        {
            options.ConfigureDbContext = o => o.UseInMemoryDatabase(databaseName);
        });

        await Assert.That(withoutRefreshServices.Any(d => d.ServiceType == typeof(IHostedService))).IsFalse();

        var withRefreshServices = new ServiceCollection();
        new ConfigurationBuilder().AddEntityFrameworkConfiguration<TestDbContext, TestConfigEntity>(withRefreshServices, options =>
        {
            options.ConfigureDbContext = o => o.UseInMemoryDatabase(databaseName);
            options.PeriodicalRefreshInterval = TimeSpan.FromSeconds(30);
        });

        await Assert.That(withRefreshServices.Any(d => d.ServiceType == typeof(IHostedService))).IsTrue();
    }
}
