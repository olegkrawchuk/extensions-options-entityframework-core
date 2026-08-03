namespace EFCoreConfigurationSample.Extensions;

internal static class ServiceCollectionsExtensions
{
    public static IServiceCollection ConfigureAndValidate<TOptions>(this IServiceCollection services, string configSectionPath) where TOptions : class
    {
        services
            .AddOptions<TOptions>()
            .BindConfiguration(configSectionPath)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
