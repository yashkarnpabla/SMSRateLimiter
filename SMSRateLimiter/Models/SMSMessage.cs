namespace SMSRateLimiter.Models; // Define the namespace for these message-related models

/// <summary>
/// Represents an SMS message to be sent
/// </summary>
public class SMSMessage // Class that represents a complete SMS message with sender, recipient, and content
{
    /// <summary>
    /// The business phone number sending the message
    /// </summary>
    public string BusinessPhoneNumber { get; set; } = string.Empty; // The sender's phone number, initialized as empty string
    
    /// <summary>
    /// The recipient's phone number
    /// </summary>
    public string RecipientPhoneNumber { get; set; } = string.Empty; // The recipient's phone number, initialized as empty string
    
    /// <summary>
    /// The message content
    /// </summary>
    public string MessageContent { get; set; } = string.Empty; // The actual text content of the message, initialized as empty string
}

/// <summary>
/// Request to check if a message can be sent
/// </summary>
public class CanSendMessageRequest // Class for the API request to check if a message can be sent without exceeding rate limits
{
    /// <summary>
    /// The business phone number sending the message
    /// </summary>
    public required string BusinessPhoneNumber { get; set; } // The sender's phone number, marked as required to ensure it's provided
}

/// <summary>
/// Response indicating whether a message can be sent
/// </summary>
public class CanSendMessageResponse // Class for the API response indicating whether a message can be sent
{
    /// <summary>
    /// Indicates whether the message can be sent without exceeding rate limits
    /// </summary>
    public bool CanSend { get; set; } // Boolean flag indicating if the message can be sent (true) or not (false)
    
    /// <summary>
    /// Reason why the message cannot be sent (if applicable)
    /// </summary>
    public string? Reason { get; set; } // Optional explanation for why the message cannot be sent, null if CanSend is true
}