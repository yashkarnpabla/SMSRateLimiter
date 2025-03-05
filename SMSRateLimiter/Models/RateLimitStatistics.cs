namespace SMSRateLimiter.Models;

/// <summary>
/// Statistics for a specific phone number
/// </summary>
public class PhoneNumberStatistics
{
    /// <summary>
    /// The business phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current messages per second rate
    /// </summary>
    public double CurrentMessagesPerSecond { get; set; }

    /// <summary>
    /// Last time a message was sent from this number
    /// </summary>
    public DateTime LastActivity { get; set; }

    /// <summary>
    /// Total messages sent from this number
    /// </summary>
    public long TotalMessagesSent { get; set; }
}

/// <summary>
/// Overall account statistics
/// </summary>
public class AccountStatistics
{
    /// <summary>
    /// Current messages per second rate across the entire account
    /// </summary>
    public double CurrentMessagesPerSecond { get; set; }

    /// <summary>
    /// Total messages sent across the account
    /// </summary>
    public long TotalMessagesSent { get; set; }

    /// <summary>
    /// Number of active phone numbers being tracked
    /// </summary>
    public int ActivePhoneNumbers { get; set; }
} 