using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class ServiceBusMessage : BaseEntity
{
    public string MessageId { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string? CorrelationId { get; private set; }
    public MessageStatus Status { get; private set; } = MessageStatus.Received;
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // EF Core constructor
    private ServiceBusMessage() { }

    public static ServiceBusMessage Create(
        string messageId,
        string body,
        string subject,
        string? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        return new ServiceBusMessage
        {
            MessageId = messageId,
            Body = body,
            Subject = subject,
            CorrelationId = correlationId,
            Status = MessageStatus.Received
        };
    }

    public void MarkAsProcessed()
    {
        Status = MessageStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = MessageStatus.Failed;
        ErrorMessage = errorMessage;
        RetryCount++;
        SetUpdated();
    }

    public void MarkAsRetrying()
    {
        Status = MessageStatus.Retrying;
        RetryCount++;
        SetUpdated();
    }
}
