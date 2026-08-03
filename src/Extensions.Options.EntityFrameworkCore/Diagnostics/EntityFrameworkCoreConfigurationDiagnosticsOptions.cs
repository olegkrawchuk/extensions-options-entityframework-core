namespace Extensions.Options.EntityFrameworkCore.Diagnostics;

/// <summary>
/// Configures the library's own opt-in diagnostics. Silent by default — no routine log messages.
/// Failures reading configuration are always logged, regardless of these settings.
/// </summary>
public class EntityFrameworkCoreConfigurationDiagnosticsOptions
{
    /// <summary>
    /// When <c>true</c>, a Debug-level message with the number of rows read is logged on every
    /// successful (re)load. Defaults to <c>false</c>.
    /// </summary>
    public bool LogReloadEvents { get; set; }
}
