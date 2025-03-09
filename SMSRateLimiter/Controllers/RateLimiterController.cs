using Microsoft.AspNetCore.Mvc; // Import the MVC namespace for controller functionality
using SMSRateLimiter.Models; // Import the Models namespace for request/response objects
using SMSRateLimiter.Services; // Import the Services namespace for the rate limiter service

namespace SMSRateLimiter.Controllers; // Define the namespace for this controller

/// <summary>
/// Controller for SMS rate limiting operations
/// </summary>
[ApiController] // Marks this class as an API controller, enabling API-specific behaviors
[Route("api/[controller]")] // Defines the base route for this controller as "api/RateLimiter"
public class RateLimiterController : ControllerBase // Inherits from ControllerBase which provides core controller functionality without view support
{
    private readonly IRateLimiterService _rateLimiterService; // Dependency injection for the rate limiter service
    private readonly ILogger<RateLimiterController> _logger; // Dependency injection for logging

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterController"/> class
    /// </summary>
    /// <param name="rateLimiterService">Rate limiter service</param>
    /// <param name="logger">Logger</param>
    public RateLimiterController(IRateLimiterService rateLimiterService, ILogger<RateLimiterController> logger) // Constructor with dependency injection
    {
        _rateLimiterService = rateLimiterService; // Initialize the rate limiter service
        _logger = logger; // Initialize the logger
    }

    /// <summary>
    /// Checks if a message can be sent from the specified business phone number
    /// </summary>
    /// <param name="request">Request containing the business phone number</param>
    /// <returns>Response indicating whether the message can be sent</returns>
    [HttpPost("can-send")] // Defines this method as handling POST requests to "api/RateLimiter/can-send"
    [ProducesResponseType(typeof(CanSendMessageResponse), StatusCodes.Status200OK)] // Documents that this action returns a 200 OK with CanSendMessageResponse
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documents that this action can return a 400 Bad Request
    public async Task<IActionResult> CanSendMessage([FromBody] CanSendMessageRequest request) // Async method that takes a request from the HTTP body
    {
        if (string.IsNullOrWhiteSpace(request.BusinessPhoneNumber)) // Validate that the business phone number is provided
        {
            return BadRequest("Business phone number is required"); // Return 400 Bad Request if validation fails
        }

        var response = await _rateLimiterService.CanSendMessageAsync(request.BusinessPhoneNumber); // Call the service to check if a message can be sent
        return Ok(response); // Return 200 OK with the response from the service
    }

    /// <summary>
    /// Records that a message was sent from the specified business phone number
    /// </summary>
    /// <param name="request">Request containing the business phone number</param>
    /// <returns>No content</returns>
    [HttpPost("record-sent")] // Defines this method as handling POST requests to "api/RateLimiter/record-sent"
    [ProducesResponseType(StatusCodes.Status204NoContent)] // Documents that this action returns a 204 No Content
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documents that this action can return a 400 Bad Request
    public async Task<IActionResult> RecordMessageSent([FromBody] CanSendMessageRequest request) // Async method that takes a request from the HTTP body
    {
        if (string.IsNullOrWhiteSpace(request.BusinessPhoneNumber)) // Validate that the business phone number is provided
        {
            return BadRequest("Business phone number is required"); // Return 400 Bad Request if validation fails
        }

        await _rateLimiterService.RecordMessageSentAsync(request.BusinessPhoneNumber); // Call the service to record that a message was sent
        return NoContent(); // Return 204 No Content to indicate successful processing without a response body
    }
}