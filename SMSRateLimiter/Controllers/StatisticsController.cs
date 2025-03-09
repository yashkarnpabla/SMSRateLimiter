using Microsoft.AspNetCore.Mvc; // Import the MVC namespace for controller functionality
using SMSRateLimiter.Services; // Import the Services namespace for the rate limiter service

namespace SMSRateLimiter.Controllers; // Define the namespace for this controller

/// <summary>
/// Controller for SMS rate limiting statistics
/// </summary>
[ApiController] // Marks this class as an API controller, enabling API-specific behaviors
[Route("api/[controller]")] // Defines the base route for this controller as "api/Statistics"
public class StatisticsController : ControllerBase // Inherits from ControllerBase which provides core controller functionality without view support
{
    private readonly IRateLimiterService _rateLimiterService; // Dependency injection for the rate limiter service
    private readonly ILogger<StatisticsController> _logger; // Dependency injection for logging

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsController"/> class
    /// </summary>
    /// <param name="rateLimiterService">Rate limiter service</param>
    /// <param name="logger">Logger</param>
    public StatisticsController(IRateLimiterService rateLimiterService, ILogger<StatisticsController> logger) // Constructor with dependency injection
    {
        _rateLimiterService = rateLimiterService; // Initialize the rate limiter service
        _logger = logger; // Initialize the logger
    }

    /// <summary>
    /// Gets statistics for a specific phone number
    /// </summary>
    /// <param name="phoneNumber">The business phone number</param>
    /// <returns>Statistics for the specified phone number</returns>
    [HttpGet("phone-number/{phoneNumber}")] // Defines this method as handling GET requests to "api/Statistics/phone-number/{phoneNumber}"
    [ProducesResponseType(StatusCodes.Status200OK)] // Documents that this action returns a 200 OK response
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Documents that this action can return a 404 Not Found response
    public async Task<IActionResult> GetPhoneNumberStatistics(string phoneNumber) // Async method that takes a phone number from the route
    {
        var statistics = await _rateLimiterService.GetPhoneNumberStatisticsAsync(phoneNumber); // Call the service to get statistics for the phone number
        if (statistics == null) // Check if statistics were found
        {
            return NotFound($"No statistics found for phone number: {phoneNumber}"); // Return 404 Not Found if no statistics exist
        }

        return Ok(statistics); // Return 200 OK with the statistics
    }

    /// <summary>
    /// Gets statistics for all phone numbers
    /// </summary>
    /// <returns>List of statistics for all phone numbers</returns>
    [HttpGet("phone-numbers")] // Defines this method as handling GET requests to "api/Statistics/phone-numbers"
    [ProducesResponseType(StatusCodes.Status200OK)] // Documents that this action returns a 200 OK response
    public async Task<IActionResult> GetAllPhoneNumberStatistics() // Async method with no parameters
    {
        var statistics = await _rateLimiterService.GetAllPhoneNumberStatisticsAsync(); // Call the service to get statistics for all phone numbers
        return Ok(statistics); // Return 200 OK with the statistics
    }

    /// <summary>
    /// Gets overall account statistics
    /// </summary>
    /// <returns>Account statistics</returns>
    [HttpGet("account")] // Defines this method as handling GET requests to "api/Statistics/account"
    [ProducesResponseType(StatusCodes.Status200OK)] // Documents that this action returns a 200 OK response
    public async Task<IActionResult> GetAccountStatistics() // Async method with no parameters
    {
        var statistics = await _rateLimiterService.GetAccountStatisticsAsync(); // Call the service to get account-level statistics
        return Ok(statistics); // Return 200 OK with the statistics
    }
}