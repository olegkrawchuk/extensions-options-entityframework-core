using Extensions.Options.EntityFrameworkCore;

namespace EFCoreConfigurationSample.Persistense.Entities;

public class DbOptionsEntity : IConfigEntity
{
    public required string Name { get; init; }
    public required string? Value { get; set; }
    public string? Description { get; set; }
}
