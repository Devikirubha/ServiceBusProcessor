namespace Application.Interfaces;

public interface IServiceBusService
{
    Task StartProcessingAsync(CancellationToken cancellationToken = default);
    Task StopProcessingAsync(CancellationToken cancellationToken = default);
}
