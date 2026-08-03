using Extensions.Options.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Extensions.Options.EntityFrameworkCore.Retry;

internal static class RetryPolicyExecutor
{
    public static T Execute<T>(Func<T> action, EntityFrameworkCoreConfigurationRetryOptions options, ILogger logger)
    {
        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                return action();
            }
            catch (Exception exception)
            {
                if (attempt >= options.MaxAttempts)
                {
                    EntityFrameworkCoreConfigurationLog.ReloadExhausted(logger, options.MaxAttempts, exception);
                    throw;
                }

                var delay = options.GetDelay(attempt);
                EntityFrameworkCoreConfigurationLog.ReloadAttemptFailed(logger, attempt, options.MaxAttempts, delay, exception);
                Thread.Sleep(delay);
            }
        }
    }
}
