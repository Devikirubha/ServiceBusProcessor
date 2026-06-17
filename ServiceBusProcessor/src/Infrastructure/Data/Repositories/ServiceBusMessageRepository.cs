using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories;

public class ServiceBusMessageRepository : IServiceBusMessageRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ServiceBusMessageRepository> _logger;

    public ServiceBusMessageRepository(
        ApplicationDbContext context,
        ILogger<ServiceBusMessageRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceBusMessage?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ServiceBusMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching message by Id {Id}", id);
            throw;
        }
    }

    public async Task<ServiceBusMessage?> GetByMessageIdAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ServiceBusMessages
                .FirstOrDefaultAsync(m => m.MessageId == messageId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching message by MessageId {MessageId}", messageId);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceBusMessage>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ServiceBusMessages
                .AsNoTracking()
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged messages");
            throw;
        }
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ServiceBusMessages.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total message count");
            throw;
        }
    }

    public async Task AddAsync(
        ServiceBusMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.ServiceBusMessages.AddAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding message {MessageId}", message.MessageId);
            throw;
        }
    }

    public void Update(ServiceBusMessage message)
    {
        try
        {
            _context.ServiceBusMessages.Update(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating message {MessageId}", message.MessageId);
            throw;
        }
    }
}
