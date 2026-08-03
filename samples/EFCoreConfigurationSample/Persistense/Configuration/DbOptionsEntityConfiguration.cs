using EFCoreConfigurationSample.Persistense.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace EFCoreConfigurationSample.Persistense.Configuration;

public class DbOptionsEntityConfiguration : IEntityTypeConfiguration<DbOptionsEntity>
{
    public void Configure(EntityTypeBuilder<DbOptionsEntity> builder)
    {
        builder.ToTable("CORE_PARAMETERS");
        builder.HasKey(p => p.Name);
    }
}
