using Microsoft.EntityFrameworkCore;
using Extensions.Options.EntityFrameworkCore;
using EFCoreConfigurationSample.Persistense.Entities;

namespace EFCoreConfigurationSample.Persistense;

internal static class DependencyInjection
{
    public static IServiceCollection AddPersistense(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<ApplicationDbContext>(c => UsePostgreSqlProvider(c, configuration));

        return services;
    }

    public static ConfigurationManager AddEntityFrameworkCoreConfiguration(
        this ConfigurationManager configurationManager, IServiceCollection services, IConfiguration configuration)
    {
        configurationManager.AddEntityFrameworkConfiguration<ApplicationDbContext, DbOptionsEntity>(services, options =>
        {
            options.ConfigureDbContext = o => UsePostgreSqlProvider(o, configuration);
            options.Filter = e => e.Value != null;
            options.PeriodicalRefreshInterval = TimeSpan.FromSeconds(15);
            options.Diagnostics.LogReloadEvents = true;
        });

        return configurationManager;
    }

    public static DbContextOptionsBuilder UsePostgreSqlProvider(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        optionsBuilder.UseNpgsql(connectionString);

        return optionsBuilder;
    }

}
