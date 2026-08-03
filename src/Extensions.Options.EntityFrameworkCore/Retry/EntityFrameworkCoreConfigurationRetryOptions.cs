using System;

namespace Extensions.Options.EntityFrameworkCore.Retry;

/// <summary>
/// The retry policy applied when reading the configuration table fails (e.g. the database is
/// temporarily unavailable). Delay for attempt <c>n</c> is
/// <c>min(InitialDelay * BackoffFactor^(n-1), MaxDelay)</c>. Attempts run synchronously
/// (<see cref="System.Threading.Thread.Sleep(TimeSpan)"/> between them) since the underlying
/// <c>ConfigurationProvider.Load()</c> contract itself is synchronous.
/// </summary>
public class EntityFrameworkCoreConfigurationRetryOptions
{
    /// <summary>The maximum number of attempts before giving up. Defaults to 3.</summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>The delay before the second attempt. Defaults to 200ms.</summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>The exponential backoff multiplier applied to each subsequent delay. Defaults to 2.0.</summary>
    public double BackoffFactor { get; set; } = 2.0;

    /// <summary>The upper bound on the delay between attempts, regardless of backoff. Defaults to 5 seconds.</summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(5);

    internal TimeSpan GetDelay(int attempt)
    {
        var delay = InitialDelay.TotalMilliseconds * Math.Pow(BackoffFactor, attempt - 1);
        return TimeSpan.FromMilliseconds(Math.Min(delay, MaxDelay.TotalMilliseconds));
    }
}
