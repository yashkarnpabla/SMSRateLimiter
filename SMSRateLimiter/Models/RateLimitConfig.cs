namespace SMSRateLimiter.Models;

/// <summary>
/// Configuration for SMS rate limits
/// </summary>
public class RateLimitConfig
{
    /// <summary>
    /// Maximum number of messages that can be sent from a single business phone number per second
    /// </summary>
    public int MaxMessagesPerNumberPerSecond { get; set; } = 5;

    /// <summary>
    /// Maximum number of messages that can be sent across the entire account per second
    /// </summary>
    public int MaxMessagesPerAccountPerSecond { get; set; } = 20;

    /// <summary>
    /// Time in minutes after which an inactive phone number's tracking data will be removed
    /// </summary>
    public int CleanupInactiveNumbersAfterMinutes { get; set; } = 60;
} 