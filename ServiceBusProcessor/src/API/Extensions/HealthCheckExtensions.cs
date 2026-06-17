using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("sql-server")
            .AddAzureServiceBusQueue(
                configuration["AzureServiceBus:ConnectionString"]!,
                configuration["AzureServiceBus:QueueName"]!,
                name: "azure-service-bus");

        return services;
    }
}
