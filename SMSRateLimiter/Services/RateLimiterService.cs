using Microsoft.Extensions.Options; // Import Options namespace for accessing configuration
using SMSRateLimiter.Models; // Import Models namespace for data types
using System.Collections.Concurrent; // Import for thread-safe collections

namespace SMSRateLimiter.Services; // Define the namespace for this service

/// <summary>
/// Implementation of the rate limiter service
/// </summary>
public class RateLimiterService : IRateLimiterService, IDisposable // Implements the rate limiter interface and IDisposable for cleanup
{
    private readonly RateLimitConfig _config; // Configuration settings for rate limits
    private readonly ILogger<RateLimiterService> _logger; // For logging information and errors
    private readonly ConcurrentDictionary<string, PhoneNumberTracker> _phoneNumberTrackers = new(); // Thread-safe dictionary to track phone numbers
    private readonly AccountTracker _accountTracker = new(); // Tracker for account-wide statistics
    private readonly Timer _cleanupTimer; // Timer for periodic cleanup of inactive phone numbers

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterService"/> class
    /// </summary>
    /// <param name="options">Rate limit configuration options</param>
    /// <param name="logger">Logger</param>
    public RateLimiterService(IOptions<RateLimitConfig> options, ILogger<RateLimiterService> logger) // Constructor with dependency injection
    {
        _config = options.Value; // Initialize configuration from options
        _logger = logger; // Initialize logger
        
        // Set up a timer to clean up inactive phone numbers
        _cleanupTimer = new Timer(CleanupInactivePhoneNumbers, null, 
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5)); // Run cleanup every 5 minutes
    }

    /// <inheritdoc />
    public async Task<CanSendMessageResponse> CanSendMessageAsync(string businessPhoneNumber) // Check if a message can be sent without exceeding rate limits
    {
        if (string.IsNullOrWhiteSpace(businessPhoneNumber)) // Validate input parameter
        {
            return new CanSendMessageResponse
            {
                CanSend = false,
                Reason = "Business phone number is required" // Return error response for invalid input
            };
        }

        var phoneNumberTracker = _phoneNumberTrackers.GetOrAdd(businessPhoneNumber, 
            _ => new PhoneNumberTracker(businessPhoneNumber)); // Get or create tracker for this phone number

        // Calculate the current account-wide rate by summing the rates of all active phone numbers
        double accountWideRate = CalculateAccountWideRate(); // Get current account-wide message rate
        
        // Check account-wide rate limit first
        if (accountWideRate >= _config.MaxMessagesPerAccountPerSecond) // If account-wide limit is exceeded
        {
            _logger.LogWarning("Account-wide rate limit exceeded: {CurrentRate}/{MaxRate} messages per second",
                accountWideRate, _config.MaxMessagesPerAccountPerSecond); // Log warning
            
            return new CanSendMessageResponse
            {
                CanSend = false,
                Reason = $"Account-wide rate limit exceeded: {accountWideRate}/{_config.MaxMessagesPerAccountPerSecond} messages per second" // Return error response
            };
        }

        // Check phone number rate limit
        if (phoneNumberTracker.CurrentRate >= _config.MaxMessagesPerNumberPerSecond) // If phone number limit is exceeded
        {
            _logger.LogWarning("Rate limit exceeded for phone number {PhoneNumber}: {CurrentRate}/{MaxRate} messages per second",
                businessPhoneNumber, phoneNumberTracker.CurrentRate, _config.MaxMessagesPerNumberPerSecond); // Log warning
            
            return new CanSendMessageResponse
            {
                CanSend = false,
                Reason = $"Rate limit exceeded for phone number: {phoneNumberTracker.CurrentRate}/{_config.MaxMessagesPerNumberPerSecond} messages per second" // Return error response
            };
        }

        return new CanSendMessageResponse { CanSend = true }; // Return success response if all checks pass
    }

    /// <inheritdoc />
    public async Task RecordMessageSentAsync(string businessPhoneNumber) // Record that a message was sent
    {
        if (string.IsNullOrWhiteSpace(businessPhoneNumber)) // Validate input parameter
        {
            throw new ArgumentException("Business phone number is required", nameof(businessPhoneNumber)); // Throw exception for invalid input
        }

        var phoneNumberTracker = _phoneNumberTrackers.GetOrAdd(businessPhoneNumber, 
            _ => new PhoneNumberTracker(businessPhoneNumber)); // Get or create tracker for this phone number

        phoneNumberTracker.RecordMessage(); // Record the message in the phone number tracker
        _accountTracker.RecordMessage(); // Record the message in the account tracker
    }

    /// <inheritdoc />
    public async Task<PhoneNumberStatistics?> GetPhoneNumberStatisticsAsync(string phoneNumber) // Get statistics for a specific phone number
    {
        if (_phoneNumberTrackers.TryGetValue(phoneNumber, out var tracker)) // Try to get the tracker for the phone number
        {
            return new PhoneNumberStatistics // Create and return statistics object
            {
                PhoneNumber = tracker.PhoneNumber,
                CurrentMessagesPerSecond = tracker.CurrentRate,
                LastActivity = tracker.LastActivity,
                TotalMessagesSent = tracker.TotalMessagesSent
            };
        }

        return null; // Return null if the phone number is not being tracked
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PhoneNumberStatistics>> GetAllPhoneNumberStatisticsAsync() // Get statistics for all phone numbers
    {
        return _phoneNumberTrackers.Values.Select(tracker => new PhoneNumberStatistics // Create and return statistics objects for all trackers
        {
            PhoneNumber = tracker.PhoneNumber,
            CurrentMessagesPerSecond = tracker.CurrentRate,
            LastActivity = tracker.LastActivity,
            TotalMessagesSent = tracker.TotalMessagesSent
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<AccountStatistics> GetAccountStatisticsAsync() // Get account-wide statistics
    {
        double accountWideRate = CalculateAccountWideRate(); // Calculate the current account-wide rate
        
        return new AccountStatistics // Create and return account statistics object
        {
            CurrentMessagesPerSecond = accountWideRate,
            TotalMessagesSent = _accountTracker.TotalMessagesSent,
            ActivePhoneNumbers = _phoneNumberTrackers.Count
        };
    }

    /// <summary>
    /// Calculates the current account-wide rate by summing the rates of all active phone numbers
    /// </summary>
    /// <returns>The current account-wide rate</returns>
    private double CalculateAccountWideRate() // Helper method to calculate total message rate across all phone numbers
    {
        return _phoneNumberTrackers.Values.Sum(tracker => tracker.CurrentRate); // Sum the rates of all phone number trackers
    }

    private void CleanupInactivePhoneNumbers(object? state) // Method called by the timer to clean up inactive phone numbers
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-_config.CleanupInactiveNumbersAfterMinutes); // Calculate cutoff time based on configuration
        var inactiveNumbers = _phoneNumberTrackers
            .Where(kvp => kvp.Value.LastActivity < cutoffTime) // Find phone numbers that haven't been active since the cutoff time
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var number in inactiveNumbers) // Remove each inactive phone number
        {
            if (_phoneNumberTrackers.TryRemove(number, out var tracker)) // Try to remove from the dictionary
            {
                _logger.LogInformation("Removed inactive phone number tracker: {PhoneNumber}, last active at {LastActivity}",
                    number, tracker.LastActivity); // Log removal
            }
        }
    }

    /// <inheritdoc />
    public void Dispose() // Implement IDisposable to clean up resources
    {
        _cleanupTimer?.Dispose(); // Dispose of the timer when the service is disposed
    }

    /// <summary>
    /// Tracks rate and statistics for a specific phone number
    /// </summary>
    private class PhoneNumberTracker // Inner class for tracking phone number statistics
    {
        private readonly object _lock = new(); // Lock object for thread safety
        private readonly Queue<DateTime> _messageTimestamps = new(); // Queue of message timestamps for rate calculation
        private const int RateWindowSeconds = 1; // Window size for rate calculation (1 second)

        /// <summary>
        /// Gets the phone number
        /// </summary>
        public string PhoneNumber { get; } // The phone number being tracked

        /// <summary>
        /// Gets the current rate of messages per second
        /// </summary>
        public double CurrentRate { get; private set; } // Current rate of messages per second

        /// <summary>
        /// Gets the last activity time
        /// </summary>
        public DateTime LastActivity { get; private set; } = DateTime.UtcNow; // Last time a message was sent

        /// <summary>
        /// Gets the total number of messages sent
        /// </summary>
        public long TotalMessagesSent { get; private set; } // Total number of messages sent

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberTracker"/> class
        /// </summary>
        /// <param name="phoneNumber">The phone number</param>
        public PhoneNumberTracker(string phoneNumber) // Constructor
        {
            PhoneNumber = phoneNumber; // Initialize the phone number
        }

        /// <summary>
        /// Records a message sent from this phone number
        /// </summary>
        public void RecordMessage() // Record a message sent from this phone number
        {
            lock (_lock) // Lock for thread safety
            {
                var now = DateTime.UtcNow; // Get current time
                LastActivity = now; // Update last activity time
                TotalMessagesSent++; // Increment total messages sent

                // Add the current timestamp
                _messageTimestamps.Enqueue(now); // Add timestamp to the queue

                // Remove timestamps older than the rate window
                var cutoff = now.AddSeconds(-RateWindowSeconds); // Calculate cutoff time
                while (_messageTimestamps.Count > 0 && _messageTimestamps.Peek() < cutoff) // Remove old timestamps
                {
                    _messageTimestamps.Dequeue();
                }

                // Calculate the current rate
                // Use the exact count of messages in the window
                CurrentRate = _messageTimestamps.Count; // Update current rate
            }
        }
    }

    /// <summary>
    /// Tracks rate and statistics for the entire account
    /// </summary>
    private class AccountTracker // Inner class for tracking account-wide statistics
    {
        private readonly object _lock = new(); // Lock object for thread safety
        private readonly Queue<DateTime> _messageTimestamps = new(); // Queue of message timestamps for rate calculation
        private const int RateWindowSeconds = 1; // Window size for rate calculation (1 second)

        /// <summary>
        /// Gets the current rate of messages per second
        /// </summary>
        public double CurrentRate { get; private set; } // Current rate of messages per second

        /// <summary>
        /// Gets the total number of messages sent
        /// </summary>
        public long TotalMessagesSent { get; private set; } // Total number of messages sent

        /// <summary>
        /// Records a message sent from any phone number
        /// </summary>
        public void RecordMessage() // Record a message sent from any phone number
        {
            lock (_lock) // Lock for thread safety
            {
                var now = DateTime.UtcNow; // Get current time
                TotalMessagesSent++; // Increment total messages sent

                // Add the current timestamp
                _messageTimestamps.Enqueue(now); // Add timestamp to the queue

                // Remove timestamps older than the rate window
                var cutoff = now.AddSeconds(-RateWindowSeconds); // Calculate cutoff time
                while (_messageTimestamps.Count > 0 && _messageTimestamps.Peek() < cutoff) // Remove old timestamps
                {
                    _messageTimestamps.Dequeue();
                }

                // Calculate the current rate
                // Use the exact count of messages in the window
                CurrentRate = _messageTimestamps.Count; // Update current rate
            }
        }
    }
}