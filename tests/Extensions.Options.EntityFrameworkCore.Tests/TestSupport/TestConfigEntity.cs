namespace Extensions.Options.EntityFrameworkCore.Tests.TestSupport;

public class TestConfigEntity : IConfigEntity
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Value { get; set; }

    public string? Description { get; set; }
}
