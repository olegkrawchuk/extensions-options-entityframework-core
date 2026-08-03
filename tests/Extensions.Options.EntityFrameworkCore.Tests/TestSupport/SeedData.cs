using Microsoft.EntityFrameworkCore;

namespace Extensions.Options.EntityFrameworkCore.Tests.TestSupport;

public static class SeedData
{
    public static readonly (string Name, string? Value)[] Entries =
    [
        ("Feature:EnableFoo", "true"),
        ("Feature:MaxRetries", "5"),
        ("ConnectionStrings:External", "some-connection-string"),
        ("Legacy:Disabled", null),
    ];

    public static readonly (string Name, string? Value)[] EntriesWithValue = Entries
        .Where(entry => entry.Value != null)
        .ToArray();

    public static void Seed(string databaseName)
    {
        using var context = new TestDbContext(CreateInMemoryOptions(databaseName));

        foreach (var (name, value) in Entries)
        {
            context.ConfigEntries.Add(new TestConfigEntity { Name = name, Value = value });
        }

        context.SaveChanges();
    }

    public static DbContextOptions<TestDbContext> CreateInMemoryOptions(string databaseName) =>
        new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(databaseName).Options;
}
