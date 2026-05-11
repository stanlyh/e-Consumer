using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonDataApp.Domain.Ports.Output;
using PersonDataApp.Infrastructure.ExternalServices;
using PersonDataApp.Infrastructure.Persistence;

namespace PersonDataApp.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPersonRepository, PersonRepository>();

        if (configuration.GetValue<bool>("UseFakeExternalService"))
        {
            services.AddScoped<IExternalPersonService, FakePersonServiceAdapter>();
        }
        else
        {
            services.AddHttpClient<ExternalPersonServiceAdapter>(client =>
                client.BaseAddress = new Uri(configuration["ExternalService:BaseUrl"]!));
            services.AddScoped<IExternalPersonService, ExternalPersonServiceAdapter>();
        }

        return services;
    }
}
