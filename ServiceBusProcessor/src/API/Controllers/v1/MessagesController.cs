using Application.Messages.DTOs;
using Application.Messages.Queries;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;

/// <summary>
/// Service Bus Messages — v1
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMediator mediator, ILogger<MessagesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Returns a paged list of Service Bus messages.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "messages:read")]
    [ProducesResponseType(typeof(PagedResult<ServiceBusMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMessages(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "GET /messages requested (page={Page}, pageSize={PageSize})", page, pageSize);

            var result = await _mediator.Send(
                new GetMessagesQuery(page, pageSize), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetMessages");
            throw; 
        }
    }

    /// <summary>
    /// Returns a single Service Bus message by its database ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "messages:read")]
    [ProducesResponseType(typeof(ServiceBusMessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMessageById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GET /messages/{Id}", id);

            var result = await _mediator.Send(
                new GetMessageByIdQuery(id), cancellationToken);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetMessageById {Id}", id);
            throw;
        }
    }
}
