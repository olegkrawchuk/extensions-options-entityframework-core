using Extensions.Options.EntityFrameworkCore.Retry;
using Microsoft.Extensions.Logging.Abstractions;

namespace Extensions.Options.EntityFrameworkCore.Tests;

public class RetryPolicyExecutorTests
{
    [Test]
    public async Task Execute_ReturnsResult_WhenFirstAttemptSucceeds()
    {
        var options = new EntityFrameworkCoreConfigurationRetryOptions
        {
            MaxAttempts = 3,
            InitialDelay = TimeSpan.Zero,
            MaxDelay = TimeSpan.Zero,
        };

        var attempts = 0;
        var result = RetryPolicyExecutor.Execute(() =>
        {
            attempts++;
            return 42;
        }, options, NullLogger.Instance);

        await Assert.That(result).IsEqualTo(42);
        await Assert.That(attempts).IsEqualTo(1);
    }

    [Test]
    public async Task Execute_RetriesUpToMaxAttempts_ThenThrows()
    {
        var options = new EntityFrameworkCoreConfigurationRetryOptions
        {
            MaxAttempts = 3,
            InitialDelay = TimeSpan.Zero,
            MaxDelay = TimeSpan.Zero,
        };

        var attempts = 0;

        void Act() => RetryPolicyExecutor.Execute<int>(() =>
        {
            attempts++;
            throw new InvalidOperationException("Simulated failure");
        }, options, NullLogger.Instance);

        await Assert.That(Act).Throws<InvalidOperationException>();
        await Assert.That(attempts).IsEqualTo(3);
    }

    [Test]
    public async Task Execute_SucceedsOnLastAllowedAttempt()
    {
        var options = new EntityFrameworkCoreConfigurationRetryOptions
        {
            MaxAttempts = 3,
            InitialDelay = TimeSpan.Zero,
            MaxDelay = TimeSpan.Zero,
        };

        var attempts = 0;
        var result = RetryPolicyExecutor.Execute(() =>
        {
            attempts++;
            return attempts < 3 ? throw new InvalidOperationException("Simulated failure") : attempts;
        }, options, NullLogger.Instance);

        await Assert.That(result).IsEqualTo(3);
        await Assert.That(attempts).IsEqualTo(3);
    }

    [Test]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(3)]
    [Arguments(4)]
    public async Task GetDelay_GrowsExponentially_UpToMaxDelay(int attempt)
    {
        var options = new EntityFrameworkCoreConfigurationRetryOptions
        {
            InitialDelay = TimeSpan.FromMilliseconds(200),
            BackoffFactor = 2.0,
            MaxDelay = TimeSpan.FromSeconds(1),
        };

        var expectedMilliseconds = Math.Min(200 * Math.Pow(2, attempt - 1), 1000);
        var delay = options.GetDelay(attempt);

        await Assert.That(delay).IsEqualTo(TimeSpan.FromMilliseconds(expectedMilliseconds));
    }
}
