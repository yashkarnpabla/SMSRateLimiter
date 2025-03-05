using System.Diagnostics;
using FluentAssertions;
using Xunit;

namespace SMSRateLimiter.IntegrationTests;

public class PerformanceTests : TestBase
{
    [Fact]
    public async Task CanSendMessage_MultipleConsecutiveRequests_CompletesWithinReasonableTime()
    {
        // Arrange
        var phoneNumber = "+1555555555";
        var stopwatch = new Stopwatch();
        const int requestCount = 10;

        // Act
        stopwatch.Start();
        
        for (int i = 0; i < requestCount; i++)
        {
            var result = await CanSendMessageAsync(phoneNumber);
            result.Should().NotBeNull();
        }
        
        stopwatch.Stop();

        // Assert
        // Each request should take less than 50ms on average
        var averageTimePerRequest = stopwatch.ElapsedMilliseconds / requestCount;
        averageTimePerRequest.Should().BeLessThan(50);
    }

    [Fact]
    public async Task RecordMessageSent_MultipleConsecutiveRequests_CompletesWithinReasonableTime()
    {
        // Arrange
        var phoneNumbers = Enumerable.Range(1, 10)
            .Select(i => $"+1666{i.ToString().PadLeft(7, '0')}")
            .ToArray();
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        
        foreach (var phoneNumber in phoneNumbers)
        {
            await RecordMessageSentAsync(phoneNumber);
        }
        
        stopwatch.Stop();

        // Assert
        // Each request should take less than 50ms on average
        var averageTimePerRequest = stopwatch.ElapsedMilliseconds / phoneNumbers.Length;
        averageTimePerRequest.Should().BeLessThan(50);
    }

    [Fact]
    public async Task GetStatistics_AfterManyMessages_CompletesWithinReasonableTime()
    {
        // Arrange
        var phoneNumber = "+1777777777";
        const int messageCount = 20;
        
        // Send multiple messages
        for (int i = 0; i < messageCount; i++)
        {
            await RecordMessageSentAsync(phoneNumber);
        }
        
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        var phoneStats = await GetPhoneNumberStatisticsAsync(phoneNumber);
        var accountStats = await GetAccountStatisticsAsync();
        var allStats = await GetAllPhoneNumberStatisticsAsync();
        stopwatch.Stop();

        // Assert
        phoneStats.Should().NotBeNull();
        accountStats.Should().NotBeNull();
        allStats.Should().NotBeNull();
        
        // All statistics requests combined should complete within 100ms
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public async Task CanSendMessage_ParallelRequests_AllComplete()
    {
        // Arrange
        var phoneNumbers = Enumerable.Range(1, 10)
            .Select(i => $"+1888{i.ToString().PadLeft(7, '0')}")
            .ToArray();
        
        // Act
        var tasks = phoneNumbers.Select(CanSendMessageAsync).ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(phoneNumbers.Length);
        results.All(r => r != null).Should().BeTrue();
    }
} 