using Microsoft.Extensions.Options;
using SMSRateLimiter.Models;
using System.Collections.Concurrent;

namespace SMSRateLimiter.Services;

/// <summary>
/// Implementation of the rate limiter service
/// </summary>
public class RateLimiterService : IRateLimiterService, IDisposable
{
    private readonly RateLimitConfig _config;
    private readonly ILogger<RateLimiterService> _logger;
    private readonly ConcurrentDictionary<string, PhoneNumberTracker> _phoneNumberTrackers = new();
    private readonly AccountTracker _accountTracker = new();
    private readonly Timer _cleanupTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterService"/> class
    /// </summary>
    /// <param name="options">Rate limit configuration options</param>
    /// <param name="logger">Logger</param>
    public RateLimiterService(IOptions<RateLimitConfig> options, ILogger<RateLimiterService> logger)
    {
        _config = options.Value;
        _logger = logger;
        
        // Set up a timer to clean up inactive phone numbers
        _cleanupTimer = new Timer(CleanupInactivePhoneNumbers, null, 
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <inheritdoc />
    public async Task<CanSendMessageResponse> CanSendMessageAsync(string businessPhoneNumber)
    {
        if (string.IsNullOrWhiteSpace(businessPhoneNumber))
        {
            return new CanSendMessageResponse
            {
                CanSend = false,
                Reason = "Business phone number is required"
            };
        }

        var phoneNumberTracker = _phoneNumberTrackers.GetOrAdd(businessPhoneNumber, 
            _ => new PhoneNumberTracker(businessPhoneNumber));

        // Calculate the current account-wide rate by summing the rates of all active phone numbers
        double accountWideRate = CalculateAccountWideRate();
        
        // Check account-wide rate limit first
        if (accountWideRate >= _config.MaxMessagesPerAccountPerSecond)
        {
            _logger.LogWarning("Account-wide rate limit exceeded: {CurrentRate}/{MaxRate} messages per second",
                accountWideRate, _config.MaxMessagesPerAccountPerSecond);
            
            return new CanSendMessageResponse
            {
                CanSend = false,
                Reason = $"Account-wide rate limit exceeded: {accountWideRate}/{_config.MaxMessagesPerAccountPerSecond} messages per second"
            };
        }

        // Check phone number rate limit
        if (phoneNumberTracker.CurrentRate >= _config.MaxMessagesPerNumberPerSecond)
        {
            _logger.LogWarning("Rate limit exceeded for phone number {PhoneNumber}: {CurrentRate}/{MaxRate} messages per second",
                businessPhoneNumber, phoneNumberTracker.CurrentRate, _config.MaxMessagesPerNumberPerSecond);
            
            return new CanSendMessageResponse
            {
                CanSend = false,
                Reason = $"Rate limit exceeded for phone number: {phoneNumberTracker.CurrentRate}/{_config.MaxMessagesPerNumberPerSecond} messages per second"
            };
        }

        return new CanSendMessageResponse { CanSend = true };
    }

    /// <inheritdoc />
    public async Task RecordMessageSentAsync(string businessPhoneNumber)
    {
        if (string.IsNullOrWhiteSpace(businessPhoneNumber))
        {
            throw new ArgumentException("Business phone number is required", nameof(businessPhoneNumber));
        }

        var phoneNumberTracker = _phoneNumberTrackers.GetOrAdd(businessPhoneNumber, 
            _ => new PhoneNumberTracker(businessPhoneNumber));

        phoneNumberTracker.RecordMessage();
        _accountTracker.RecordMessage();
    }

    /// <inheritdoc />
    public async Task<PhoneNumberStatistics?> GetPhoneNumberStatisticsAsync(string phoneNumber)
    {
        if (_phoneNumberTrackers.TryGetValue(phoneNumber, out var tracker))
        {
            return new PhoneNumberStatistics
            {
                PhoneNumber = tracker.PhoneNumber,
                CurrentMessagesPerSecond = tracker.CurrentRate,
                LastActivity = tracker.LastActivity,
                TotalMessagesSent = tracker.TotalMessagesSent
            };
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PhoneNumberStatistics>> GetAllPhoneNumberStatisticsAsync()
    {
        return _phoneNumberTrackers.Values.Select(tracker => new PhoneNumberStatistics
        {
            PhoneNumber = tracker.PhoneNumber,
            CurrentMessagesPerSecond = tracker.CurrentRate,
            LastActivity = tracker.LastActivity,
            TotalMessagesSent = tracker.TotalMessagesSent
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<AccountStatistics> GetAccountStatisticsAsync()
    {
        double accountWideRate = CalculateAccountWideRate();
        
        return new AccountStatistics
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
    private double CalculateAccountWideRate()
    {
        return _phoneNumberTrackers.Values.Sum(tracker => tracker.CurrentRate);
    }

    private void CleanupInactivePhoneNumbers(object? state)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-_config.CleanupInactiveNumbersAfterMinutes);
        var inactiveNumbers = _phoneNumberTrackers
            .Where(kvp => kvp.Value.LastActivity < cutoffTime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var number in inactiveNumbers)
        {
            if (_phoneNumberTrackers.TryRemove(number, out var tracker))
            {
                _logger.LogInformation("Removed inactive phone number tracker: {PhoneNumber}, last active at {LastActivity}",
                    number, tracker.LastActivity);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }

    /// <summary>
    /// Tracks rate and statistics for a specific phone number
    /// </summary>
    private class PhoneNumberTracker
    {
        private readonly object _lock = new();
        private readonly Queue<DateTime> _messageTimestamps = new();
        private const int RateWindowSeconds = 1;

        /// <summary>
        /// Gets the phone number
        /// </summary>
        public string PhoneNumber { get; }

        /// <summary>
        /// Gets the current rate of messages per second
        /// </summary>
        public double CurrentRate { get; private set; }

        /// <summary>
        /// Gets the last activity time
        /// </summary>
        public DateTime LastActivity { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets the total number of messages sent
        /// </summary>
        public long TotalMessagesSent { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberTracker"/> class
        /// </summary>
        /// <param name="phoneNumber">The phone number</param>
        public PhoneNumberTracker(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        /// <summary>
        /// Records a message sent from this phone number
        /// </summary>
        public void RecordMessage()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                LastActivity = now;
                TotalMessagesSent++;

                // Add the current timestamp
                _messageTimestamps.Enqueue(now);

                // Remove timestamps older than the rate window
                var cutoff = now.AddSeconds(-RateWindowSeconds);
                while (_messageTimestamps.Count > 0 && _messageTimestamps.Peek() < cutoff)
                {
                    _messageTimestamps.Dequeue();
                }

                // Calculate the current rate
                // Use the exact count of messages in the window
                CurrentRate = _messageTimestamps.Count;
            }
        }
    }

    /// <summary>
    /// Tracks rate and statistics for the entire account
    /// </summary>
    private class AccountTracker
    {
        private readonly object _lock = new();
        private readonly Queue<DateTime> _messageTimestamps = new();
        private const int RateWindowSeconds = 1;

        /// <summary>
        /// Gets the current rate of messages per second
        /// </summary>
        public double CurrentRate { get; private set; }

        /// <summary>
        /// Gets the total number of messages sent
        /// </summary>
        public long TotalMessagesSent { get; private set; }

        /// <summary>
        /// Records a message sent from any phone number
        /// </summary>
        public void RecordMessage()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                TotalMessagesSent++;

                // Add the current timestamp
                _messageTimestamps.Enqueue(now);

                // Remove timestamps older than the rate window
                var cutoff = now.AddSeconds(-RateWindowSeconds);
                while (_messageTimestamps.Count > 0 && _messageTimestamps.Peek() < cutoff)
                {
                    _messageTimestamps.Dequeue();
                }

                // Calculate the current rate
                CurrentRate = _messageTimestamps.Count;
            }
        }
    }
} 