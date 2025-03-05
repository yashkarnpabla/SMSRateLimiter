using Microsoft.AspNetCore.Mvc;
using SMSRateLimiter.Services;

namespace SMSRateLimiter.Controllers;

/// <summary>
/// Controller for SMS rate limiting statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IRateLimiterService _rateLimiterService;
    private readonly ILogger<StatisticsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsController"/> class
    /// </summary>
    /// <param name="rateLimiterService">Rate limiter service</param>
    /// <param name="logger">Logger</param>
    public StatisticsController(IRateLimiterService rateLimiterService, ILogger<StatisticsController> logger)
    {
        _rateLimiterService = rateLimiterService;
        _logger = logger;
    }

    /// <summary>
    /// Gets statistics for a specific phone number
    /// </summary>
    /// <param name="phoneNumber">The business phone number</param>
    /// <returns>Statistics for the specified phone number</returns>
    [HttpGet("phone-number/{phoneNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPhoneNumberStatistics(string phoneNumber)
    {
        var statistics = await _rateLimiterService.GetPhoneNumberStatisticsAsync(phoneNumber);
        if (statistics == null)
        {
            return NotFound($"No statistics found for phone number: {phoneNumber}");
        }

        return Ok(statistics);
    }

    /// <summary>
    /// Gets statistics for all phone numbers
    /// </summary>
    /// <returns>List of statistics for all phone numbers</returns>
    [HttpGet("phone-numbers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPhoneNumberStatistics()
    {
        var statistics = await _rateLimiterService.GetAllPhoneNumberStatisticsAsync();
        return Ok(statistics);
    }

    /// <summary>
    /// Gets overall account statistics
    /// </summary>
    /// <returns>Account statistics</returns>
    [HttpGet("account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountStatistics()
    {
        var statistics = await _rateLimiterService.GetAccountStatisticsAsync();
        return Ok(statistics);
    }
} 