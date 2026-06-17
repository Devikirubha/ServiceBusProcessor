using Application.Messages.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Messages.Commands;

public record ProcessServiceBusMessageCommand(
    string MessageId,
    string Body,
    string Subject,
    string? CorrelationId
) : IRequest<Result<ServiceBusMessageDto>>;
