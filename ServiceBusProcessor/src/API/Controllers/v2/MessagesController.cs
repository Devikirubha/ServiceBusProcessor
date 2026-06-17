using Application.Messages.DTOs;
using Application.Messages.Queries;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v2;

/// <summary>
/// Service Bus Messages — v2 (enhanced API envelope)
/// </summary>
[ApiController]
[ApiVersion("2.0")]
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
    /// Returns a paged list of messages wrapped in an API envelope.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "messages:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMessages(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(
                new GetMessagesQuery(page, pageSize), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(Envelope.Failure(result.Error!));

            return Ok(Envelope.Ok(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in v2 GetMessages");
            throw;
        }
    }

    /// <summary>
    /// Returns a single message by ID wrapped in an API envelope.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "messages:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessageById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(
                new GetMessageByIdQuery(id), cancellationToken);

            if (!result.IsSuccess)
                return NotFound(Envelope.Failure(result.Error!));

            return Ok(Envelope.Ok(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in v2 GetMessageById {Id}", id);
            throw;
        }
    }
    /// <summary>
    /// Admin-only: returns system metadata. Requires Admin role.
    /// </summary>
    [HttpGet("admin/summary")]
    [Authorize(Policy = "admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAdminSummary()
    {
        try
        {
            var summary = new
            {
                ApiVersion = "2.0",
                ServerTime = DateTime.UtcNow,
                CallerIdentity = User.Identity?.Name ?? "Unknown"
            };
            return Ok(Envelope.Ok(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAdminSummary");
            throw;
        }
    }
}

// ── Response envelope ─────
public record Envelope<T>(
    bool Success,
    T? Data,
    string? Error,
    DateTime Timestamp)
{
    public static Envelope<T> Ok(T data) =>
        new(true, data, null, DateTime.UtcNow);

    public static Envelope<T> Failure(string error) =>
        new(false, default, error, DateTime.UtcNow);
}

public static class Envelope
{
    public static Envelope<T> Ok<T>(T data) =>
        Envelope<T>.Ok(data);

    public static Envelope<object> Failure(string error) =>
        Envelope<object>.Failure(error);
}
