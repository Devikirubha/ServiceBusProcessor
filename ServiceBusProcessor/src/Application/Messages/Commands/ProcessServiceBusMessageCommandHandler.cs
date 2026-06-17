using Application.Messages.DTOs;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Messages.Commands;

public class ProcessServiceBusMessageCommandHandler
    : IRequestHandler<ProcessServiceBusMessageCommand, Result<ServiceBusMessageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessServiceBusMessageCommandHandler> _logger;

    public ProcessServiceBusMessageCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ProcessServiceBusMessageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ServiceBusMessageDto>> Handle(
        ProcessServiceBusMessageCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing Service Bus message {MessageId} with subject {Subject}",
                request.MessageId, request.Subject);

            // Idempotency check
            var existing = await _unitOfWork.ServiceBusMessages
                .GetByMessageIdAsync(request.MessageId, cancellationToken);

            if (existing is not null)
            {
                _logger.LogWarning(
                    "Message {MessageId} already processed, returning existing record",
                    request.MessageId);
                return Result<ServiceBusMessageDto>.Success(MapToDto(existing));
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var message = ServiceBusMessage.Create(
                request.MessageId,
                request.Body,
                request.Subject,
                request.CorrelationId);

            await _unitOfWork.ServiceBusMessages.AddAsync(message, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            message.MarkAsProcessed();
            _unitOfWork.ServiceBusMessages.Update(message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully processed message {MessageId}", request.MessageId);

            return Result<ServiceBusMessageDto>.Success(MapToDto(message));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(ex,
                "Failed to process Service Bus message {MessageId}", request.MessageId);

            return Result<ServiceBusMessageDto>.Failure(
                $"Failed to process message: {ex.Message}");
        }
    }

    private static ServiceBusMessageDto MapToDto(ServiceBusMessage m) => new(
        m.Id, m.MessageId, m.Body, m.Subject,
        m.CorrelationId, m.Status, m.ErrorMessage,
        m.RetryCount, m.CreatedAt, m.ProcessedAt);
}
