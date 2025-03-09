using SMSRateLimiter.Models; // Import the Models namespace for request/response and statistics types

namespace SMSRateLimiter.Services; // Define the namespace for this service interface

/// <summary>
/// Service for checking SMS rate limits
/// </summary>
public interface IRateLimiterService // Interface defining the contract for rate limiting services
{
    /// <summary>
    /// Checks if a message can be sent from the specified business phone number
    /// </summary>
    /// <param name="businessPhoneNumber">The business phone number</param>
    /// <returns>Response indicating whether the message can be sent</returns>
    Task<CanSendMessageResponse> CanSendMessageAsync(string businessPhoneNumber); // Async method to check if a message can be sent without exceeding rate limits

    /// <summary>
    /// Records that a message was sent from the specified business phone number
    /// </summary>
    /// <param name="businessPhoneNumber">The business phone number</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task RecordMessageSentAsync(string businessPhoneNumber); // Async method to record that a message was sent, updating rate tracking data

    /// <summary>
    /// Gets statistics for a specific phone number
    /// </summary>
    /// <param name="phoneNumber">The business phone number</param>
    /// <returns>Statistics for the specified phone number</returns>
    Task<PhoneNumberStatistics?> GetPhoneNumberStatisticsAsync(string phoneNumber); // Async method to retrieve statistics for a specific phone number, returns null if not found

    /// <summary>
    /// Gets statistics for all phone numbers
    /// </summary>
    /// <returns>List of statistics for all phone numbers</returns>
    Task<IEnumerable<PhoneNumberStatistics>> GetAllPhoneNumberStatisticsAsync(); // Async method to retrieve statistics for all tracked phone numbers

    /// <summary>
    /// Gets overall account statistics
    /// </summary>
    /// <returns>Account statistics</returns>
    Task<AccountStatistics> GetAccountStatisticsAsync(); // Async method to retrieve account-wide statistics across all phone numbers
}