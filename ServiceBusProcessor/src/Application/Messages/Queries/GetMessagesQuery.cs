using Application.Messages.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Messages.Queries;

public record GetMessagesQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<ServiceBusMessageDto>>>;

public record GetMessageByIdQuery(Guid Id)
    : IRequest<Result<ServiceBusMessageDto>>;
