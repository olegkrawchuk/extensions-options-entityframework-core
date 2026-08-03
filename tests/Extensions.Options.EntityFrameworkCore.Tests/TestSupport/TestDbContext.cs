using Microsoft.EntityFrameworkCore;

namespace Extensions.Options.EntityFrameworkCore.Tests.TestSupport;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestConfigEntity> ConfigEntries => Set<TestConfigEntity>();
}
