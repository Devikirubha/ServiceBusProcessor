using Application.Messages.DTOs;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Messages.Queries;

public class GetMessagesQueryHandler
    : IRequestHandler<GetMessagesQuery, Result<PagedResult<ServiceBusMessageDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetMessagesQueryHandler> _logger;

    public GetMessagesQueryHandler(IUnitOfWork unitOfWork, ILogger<GetMessagesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ServiceBusMessageDto>>> Handle(
        GetMessagesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var messages = await _unitOfWork.ServiceBusMessages
                .GetAllAsync(request.Page, request.PageSize, cancellationToken);

            var total = await _unitOfWork.ServiceBusMessages
                .GetTotalCountAsync(cancellationToken);

            var dtos = messages.Select(MapToDto);

            return Result<PagedResult<ServiceBusMessageDto>>.Success(
                new PagedResult<ServiceBusMessageDto>(dtos, total, request.Page, request.PageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve messages");
            return Result<PagedResult<ServiceBusMessageDto>>.Failure(ex.Message);
        }
    }

    private static ServiceBusMessageDto MapToDto(ServiceBusMessage m) => new(
        m.Id, m.MessageId, m.Body, m.Subject,
        m.CorrelationId, m.Status, m.ErrorMessage,
        m.RetryCount, m.CreatedAt, m.ProcessedAt);
}

public class GetMessageByIdQueryHandler
    : IRequestHandler<GetMessageByIdQuery, Result<ServiceBusMessageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetMessageByIdQueryHandler> _logger;

    public GetMessageByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetMessageByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ServiceBusMessageDto>> Handle(
        GetMessageByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = await _unitOfWork.ServiceBusMessages
                .GetByIdAsync(request.Id, cancellationToken);

            if (message is null)
                return Result<ServiceBusMessageDto>.Failure($"Message {request.Id} not found.");

            return Result<ServiceBusMessageDto>.Success(new ServiceBusMessageDto(
                message.Id, message.MessageId, message.Body, message.Subject,
                message.CorrelationId, message.Status, message.ErrorMessage,
                message.RetryCount, message.CreatedAt, message.ProcessedAt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve message {Id}", request.Id);
            return Result<ServiceBusMessageDto>.Failure(ex.Message);
        }
    }
}
