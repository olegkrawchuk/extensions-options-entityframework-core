namespace Extensions.Options.EntityFrameworkCore;

public interface IConfigEntity
{
    string Name { get; }
    string? Value { get; }
}
