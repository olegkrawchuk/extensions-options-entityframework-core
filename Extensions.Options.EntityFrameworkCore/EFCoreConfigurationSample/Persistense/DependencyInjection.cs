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

    public static ConfigurationManager AddEntityFrameworkCoreConfiguration(this ConfigurationManager configurationManager, IServiceCollection services)
    {
        configurationManager.AddEntityFramework<DbOptionsEntity>(builder =>
        {
            var scope = services.BuildServiceProvider().CreateScope();

            builder
                .UseGetDbContextFunc(() => scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                .UseQueryFilter(e => e.Value != null)
                .EnablePeriodicalAutoRefresh(TimeSpan.FromSeconds(5));
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
