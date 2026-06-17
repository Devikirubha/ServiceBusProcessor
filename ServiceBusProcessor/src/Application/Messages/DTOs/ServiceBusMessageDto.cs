using Domain.Enums;

namespace Application.Messages.DTOs;

public record ServiceBusMessageDto(
    Guid Id,
    string MessageId,
    string Body,
    string Subject,
    string? CorrelationId,
    MessageStatus Status,
    string? ErrorMessage,
    int RetryCount,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
