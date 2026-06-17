using Domain.Entities;

namespace Domain.Interfaces;

public interface IServiceBusMessageRepository
{
    Task<ServiceBusMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceBusMessage?> GetByMessageIdAsync(string messageId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceBusMessage>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ServiceBusMessage message, CancellationToken cancellationToken = default);
    void Update(ServiceBusMessage message);
}
