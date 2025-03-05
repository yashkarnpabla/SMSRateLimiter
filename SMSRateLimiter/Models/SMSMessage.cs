namespace SMSRateLimiter.Models;

/// <summary>
/// Represents an SMS message to be sent
/// </summary>
public class SMSMessage
{
    /// <summary>
    /// The business phone number sending the message
    /// </summary>
    public string BusinessPhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// The recipient's phone number
    /// </summary>
    public string RecipientPhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// The message content
    /// </summary>
    public string MessageContent { get; set; } = string.Empty;
}

/// <summary>
/// Request to check if a message can be sent
/// </summary>
public class CanSendMessageRequest
{
    /// <summary>
    /// The business phone number sending the message
    /// </summary>
    public required string BusinessPhoneNumber { get; set; }
}

/// <summary>
/// Response indicating whether a message can be sent
/// </summary>
public class CanSendMessageResponse
{
    /// <summary>
    /// Indicates whether the message can be sent without exceeding rate limits
    /// </summary>
    public bool CanSend { get; set; }

    /// <summary>
    /// Reason why the message cannot be sent (if applicable)
    /// </summary>
    public string? Reason { get; set; }
} 