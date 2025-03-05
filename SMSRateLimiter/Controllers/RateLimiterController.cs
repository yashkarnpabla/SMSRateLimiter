using Microsoft.AspNetCore.Mvc;
using SMSRateLimiter.Models;
using SMSRateLimiter.Services;

namespace SMSRateLimiter.Controllers;

/// <summary>
/// Controller for SMS rate limiting operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RateLimiterController : ControllerBase
{
    private readonly IRateLimiterService _rateLimiterService;
    private readonly ILogger<RateLimiterController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterController"/> class
    /// </summary>
    /// <param name="rateLimiterService">Rate limiter service</param>
    /// <param name="logger">Logger</param>
    public RateLimiterController(IRateLimiterService rateLimiterService, ILogger<RateLimiterController> logger)
    {
        _rateLimiterService = rateLimiterService;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a message can be sent from the specified business phone number
    /// </summary>
    /// <param name="request">Request containing the business phone number</param>
    /// <returns>Response indicating whether the message can be sent</returns>
    [HttpPost("can-send")]
    [ProducesResponseType(typeof(CanSendMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CanSendMessage([FromBody] CanSendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BusinessPhoneNumber))
        {
            return BadRequest("Business phone number is required");
        }

        var response = await _rateLimiterService.CanSendMessageAsync(request.BusinessPhoneNumber);
        return Ok(response);
    }

    /// <summary>
    /// Records that a message was sent from the specified business phone number
    /// </summary>
    /// <param name="request">Request containing the business phone number</param>
    /// <returns>No content</returns>
    [HttpPost("record-sent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordMessageSent([FromBody] CanSendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BusinessPhoneNumber))
        {
            return BadRequest("Business phone number is required");
        }

        await _rateLimiterService.RecordMessageSentAsync(request.BusinessPhoneNumber);
        return NoContent();
    }
} 