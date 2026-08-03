using Microsoft.Extensions.Logging;
using System;

namespace Extensions.Options.EntityFrameworkCore.Diagnostics;

internal static partial class EntityFrameworkCoreConfigurationLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Reload succeeded, {RowCount} rows loaded.")]
    public static partial void ReloadSucceeded(ILogger logger, int rowCount);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Reload attempt {Attempt}/{MaxAttempts} failed, retrying in {Delay}.")]
    public static partial void ReloadAttemptFailed(ILogger logger, int attempt, int maxAttempts, TimeSpan delay, Exception exception);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Reload failed after {MaxAttempts} attempts.")]
    public static partial void ReloadExhausted(ILogger logger, int maxAttempts, Exception exception);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Initial configuration load failed before the diagnostics logger was attached; configuration started with empty/previous data.")]
    public static partial void InitialLoadFailedDeferredNotice(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Periodical configuration refresh started, interval {Interval}.")]
    public static partial void PeriodicalRefreshStarted(ILogger logger, TimeSpan interval);
}
