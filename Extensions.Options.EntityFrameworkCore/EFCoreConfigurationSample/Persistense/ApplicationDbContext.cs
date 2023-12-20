using EFCoreConfigurationSample.Persistense.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreConfigurationSample.Persistense;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<DbOptionsEntity> Options => Set<DbOptionsEntity>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        return this.Database.IsRelational()
            ? this.Database.MigrateAsync(cancellationToken)
            : Task.CompletedTask;
    }
}
