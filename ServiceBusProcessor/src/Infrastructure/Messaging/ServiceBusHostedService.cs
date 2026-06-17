using Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging;

public class ServiceBusHostedService : IHostedService
{
    private readonly IServiceBusService _serviceBusService;
    private readonly ILogger<ServiceBusHostedService> _logger;

    public ServiceBusHostedService(
        IServiceBusService serviceBusService,
        ILogger<ServiceBusHostedService> logger)
    {
        _serviceBusService = serviceBusService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting Service Bus Hosted Service...");
            await _serviceBusService.StartProcessingAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service Bus Hosted Service failed to start.");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stopping Service Bus Hosted Service...");
            await _serviceBusService.StopProcessingAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service Bus Hosted Service failed to stop cleanly.");
        }
    }
}
