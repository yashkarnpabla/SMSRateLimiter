using SMSRateLimiter.Models;

namespace SMSRateLimiter.Services;

/// <summary>
/// Service for checking SMS rate limits
/// </summary>
public interface IRateLimiterService
{
    /// <summary>
    /// Checks if a message can be sent from the specified business phone number
    /// </summary>
    /// <param name="businessPhoneNumber">The business phone number</param>
    /// <returns>Response indicating whether the message can be sent</returns>
    Task<CanSendMessageResponse> CanSendMessageAsync(string businessPhoneNumber);

    /// <summary>
    /// Records that a message was sent from the specified business phone number
    /// </summary>
    /// <param name="businessPhoneNumber">The business phone number</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task RecordMessageSentAsync(string businessPhoneNumber);

    /// <summary>
    /// Gets statistics for a specific phone number
    /// </summary>
    /// <param name="phoneNumber">The business phone number</param>
    /// <returns>Statistics for the specified phone number</returns>
    Task<PhoneNumberStatistics?> GetPhoneNumberStatisticsAsync(string phoneNumber);

    /// <summary>
    /// Gets statistics for all phone numbers
    /// </summary>
    /// <returns>List of statistics for all phone numbers</returns>
    Task<IEnumerable<PhoneNumberStatistics>> GetAllPhoneNumberStatisticsAsync();

    /// <summary>
    /// Gets overall account statistics
    /// </summary>
    /// <returns>Account statistics</returns>
    Task<AccountStatistics> GetAccountStatisticsAsync();
} 