using System.Diagnostics; // Import for performance measurement
using FluentAssertions; // Import for fluent assertions to make tests more readable
using Xunit; // Import for xUnit testing framework

namespace SMSRateLimiter.IntegrationTests; // Define the namespace for integration tests

public class PerformanceTests : TestBase // Test class for performance testing, inherits from TestBase
{
    [Fact] // Attribute to mark this method as a test
    public async Task CanSendMessage_MultipleConsecutiveRequests_CompletesWithinReasonableTime() // Test that verifies multiple consecutive can-send requests complete quickly
    {
        // Arrange
        var phoneNumber = "+1555555555"; // Define a test phone number
        var stopwatch = new Stopwatch(); // Create a stopwatch for timing
        const int requestCount = 10; // Number of requests to make

        // Act
        stopwatch.Start(); // Start timing
        
        for (int i = 0; i < requestCount; i++) // Loop to make multiple requests
        {
            var result = await CanSendMessageAsync(phoneNumber); // Call the API to check if a message can be sent
            result.Should().NotBeNull(); // Verify each result is not null
        }
        
        stopwatch.Stop(); // Stop timing

        // Assert
        // Each request should take less than 50ms on average
        var averageTimePerRequest = stopwatch.ElapsedMilliseconds / requestCount; // Calculate average time per request
        averageTimePerRequest.Should().BeLessThan(50); // Verify the average time is less than 50ms
    }

    [Fact] // Attribute to mark this method as a test
    public async Task RecordMessageSent_MultipleConsecutiveRequests_CompletesWithinReasonableTime() // Test that verifies multiple consecutive record-sent requests complete quickly
    {
        // Arrange
        var phoneNumbers = Enumerable.Range(1, 10) // Generate 10 different phone numbers
            .Select(i => $"+1666{i.ToString().PadLeft(7, '0')}") // Format each number as a phone number
            .ToArray();
        var stopwatch = new Stopwatch(); // Create a stopwatch for timing

        // Act
        stopwatch.Start(); // Start timing
        
        foreach (var phoneNumber in phoneNumbers) // Loop through all phone numbers
        {
            await RecordMessageSentAsync(phoneNumber); // Call the API to record a message as sent
        }
        
        stopwatch.Stop(); // Stop timing

        // Assert
        // Each request should take less than 50ms on average
        var averageTimePerRequest = stopwatch.ElapsedMilliseconds / phoneNumbers.Length; // Calculate average time per request
        averageTimePerRequest.Should().BeLessThan(50); // Verify the average time is less than 50ms
    }

    [Fact] // Attribute to mark this method as a test
    public async Task GetStatistics_AfterManyMessages_CompletesWithinReasonableTime() // Test that verifies getting statistics after many messages is fast
    {
        // Arrange
        var phoneNumber = "+1777777777"; // Define a test phone number
        const int messageCount = 20; // Number of messages to send
        
        // Send multiple messages
        for (int i = 0; i < messageCount; i++) // Loop to send multiple messages
        {
            await RecordMessageSentAsync(phoneNumber); // Record each message as sent
        }
        
        var stopwatch = new Stopwatch(); // Create a stopwatch for timing

        // Act
        stopwatch.Start(); // Start timing
        var phoneStats = await GetPhoneNumberStatisticsAsync(phoneNumber); // Get statistics for the phone number
        var accountStats = await GetAccountStatisticsAsync(); // Get account-wide statistics
        var allStats = await GetAllPhoneNumberStatisticsAsync(); // Get statistics for all phone numbers
        stopwatch.Stop(); // Stop timing

        // Assert
        phoneStats.Should().NotBeNull(); // Verify phone statistics are not null
        accountStats.Should().NotBeNull(); // Verify account statistics are not null
        allStats.Should().NotBeNull(); // Verify all phone number statistics are not null
        
        // All statistics requests combined should complete within 100ms
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Verify the total time is less than 100ms
    }

    [Fact] // Attribute to mark this method as a test
    public async Task CanSendMessage_ParallelRequests_AllComplete() // Test that verifies parallel can-send requests all complete successfully
    {
        // Arrange
        var phoneNumbers = Enumerable.Range(1, 10) // Generate 10 different phone numbers
            .Select(i => $"+1888{i.ToString().PadLeft(7, '0')}") // Format each number as a phone number
            .ToArray();
        
        // Act
        var tasks = phoneNumbers.Select(CanSendMessageAsync).ToArray(); // Create an array of tasks, one for each phone number
        var results = await Task.WhenAll(tasks); // Wait for all tasks to complete

        // Assert
        results.Should().HaveCount(phoneNumbers.Length); // Verify we got the expected number of results
        results.All(r => r != null).Should().BeTrue(); // Verify all results are not null
    }
}