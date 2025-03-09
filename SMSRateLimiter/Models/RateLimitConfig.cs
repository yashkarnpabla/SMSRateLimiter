namespace SMSRateLimiter.Models; // Define the namespace for this model

/// <summary>
/// Configuration for SMS rate limits
/// </summary>
public class RateLimitConfig // Class that defines configuration parameters for rate limiting
{
    /// <summary>
    /// Maximum number of messages that can be sent from a single business phone number per second
    /// </summary>
    public int MaxMessagesPerNumberPerSecond { get; set; } = 5; // Default limit of 5 messages per second per phone number
    
    /// <summary>
    /// Maximum number of messages that can be sent across the entire account per second
    /// </summary>
    public int MaxMessagesPerAccountPerSecond { get; set; } = 20; // Default limit of 20 messages per second across the entire account
    
    /// <summary>
    /// Time in minutes after which an inactive phone number's tracking data will be removed
    /// </summary>
    public int CleanupInactiveNumbersAfterMinutes { get; set; } = 60; // Default cleanup time of 60 minutes (1 hour) for inactive numbers
}