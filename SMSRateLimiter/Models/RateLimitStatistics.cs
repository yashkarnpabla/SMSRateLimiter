namespace SMSRateLimiter.Models; // Define the namespace for these statistics models

/// <summary>
/// Statistics for a specific phone number
/// </summary>
public class PhoneNumberStatistics // Class that holds statistics for an individual business phone number
{
    /// <summary>
    /// The business phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty; // The phone number identifier, initialized as empty string
    
    /// <summary>
    /// Current messages per second rate
    /// </summary>
    public double CurrentMessagesPerSecond { get; set; } // Current rate of messages being sent from this number
    
    /// <summary>
    /// Last time a message was sent from this number
    /// </summary>
    public DateTime LastActivity { get; set; } // Timestamp of the most recent message sent from this number
    
    /// <summary>
    /// Total messages sent from this number
    /// </summary>
    public long TotalMessagesSent { get; set; } // Cumulative count of all messages sent from this number
}

/// <summary>
/// Overall account statistics
/// </summary>
public class AccountStatistics // Class that holds statistics for the entire account across all phone numbers
{
    /// <summary>
    /// Current messages per second rate across the entire account
    /// </summary>
    public double CurrentMessagesPerSecond { get; set; } // Current rate of messages being sent across all phone numbers
    
    /// <summary>
    /// Total messages sent across the account
    /// </summary>
    public long TotalMessagesSent { get; set; } // Cumulative count of all messages sent across all phone numbers
    
    /// <summary>
    /// Number of active phone numbers being tracked
    /// </summary>
    public int ActivePhoneNumbers { get; set; } // Count of phone numbers that are currently being tracked
}