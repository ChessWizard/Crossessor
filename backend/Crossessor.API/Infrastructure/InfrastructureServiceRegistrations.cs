namespace Crossessor.API.Infrastructure;

public static partial class InfrastructureServiceRegistrations
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
        => services.AddCrossessorDbContext(configuration)
                   .AddInterceptors()
                   .AddLibraries()
                   .AddSettingConfigurations(configuration)
                   .AddSemanticKernel(configuration);

    public static void ConfigureInfrastructureServices(this WebApplication app)
    {
        ConfigureLibraries(app);
    }
}