using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─── Database ──────
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null));
        });

        // ─── Repository ────
        services.AddScoped<IServiceBusMessageRepository, ServiceBusMessageRepository>();

        // ─── Unit of Work ──
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ─── Service Bus ───
        services.AddSingleton<IServiceBusService, AzureServiceBusService>();

        // HOSTED SERVICE — manages the background processor lifecycle (start/stop).
        services.AddHostedService<ServiceBusHostedService>();

        return services;
    }

    public static IServiceCollection AddSerilogLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .CreateLogger();

        services.AddSingleton(Log.Logger);
        return services;
    }
}
