namespace Extensions.Options.EntityFrameworkCore;

/// <summary>
/// Contract for an EF Core entity that represents a single row of the configuration table — a
/// key/value pair. Implemented by your own entity (<c>TConfigEntity</c> in
/// <see cref="EntityFrameworkCoreConfigurationBuilderExtensions.AddEntityFrameworkConfiguration"/>);
/// any extra entity fields (e.g. a description or a last-modified timestamp) are simply ignored.
/// </summary>
public interface IConfigEntity
{
    /// <summary>The configuration key — becomes a key in <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.</summary>
    string Name { get; }

    /// <summary>The configuration value. <c>null</c> means no value is set for this key.</summary>
    string? Value { get; }
}
