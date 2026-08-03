using Extensions.Options.EntityFrameworkCore.Options;
using Extensions.Options.EntityFrameworkCore.Tests.TestSupport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Extensions.Options.EntityFrameworkCore.Tests;

public class OptionsTests
{
    [Test]
    public async Task AddEntityFrameworkConfiguration_Throws_WhenConfigureDbContextNotSet()
    {
        var configurationBuilder = new ConfigurationBuilder();
        var services = new ServiceCollection();

        void Act() => configurationBuilder.AddEntityFrameworkConfiguration<TestDbContext, TestConfigEntity>(
            services,
            _ => { });

        await Assert.That(Act).Throws<ArgumentException>();
    }

    [Test]
    public async Task Options_HaveExpectedDefaults()
    {
        var options = new EntityFrameworkConfigurationOptions<TestDbContext, TestConfigEntity>();

        await Assert.That(options.PeriodicalRefreshInterval).IsNull();
        await Assert.That(options.Filter).IsNull();
        await Assert.That(options.DbContextFactory).IsNull();
        await Assert.That(options.Diagnostics.LogReloadEvents).IsFalse();
        await Assert.That(options.Retry.MaxAttempts).IsEqualTo(3);
        await Assert.That(options.Retry.InitialDelay).IsEqualTo(TimeSpan.FromMilliseconds(200));
        await Assert.That(options.Retry.BackoffFactor).IsEqualTo(2.0);
        await Assert.That(options.Retry.MaxDelay).IsEqualTo(TimeSpan.FromSeconds(5));
    }
}
