using System.Net;
using FluentAssertions;
using Xunit;

namespace SMSRateLimiter.IntegrationTests;

public class EdgeCaseTests : TestBase
{
    [Fact]
    public async Task CanSendMessage_WithInvalidPhoneNumberFormat_StillWorks()
    {
        // Arrange
        var phoneNumber = "invalid-format";

        // Act
        var result = await CanSendMessageAsync(phoneNumber);

        // Assert
        result.Should().NotBeNull();
        result.CanSend.Should().BeTrue(); // Should still work as we don't validate format
    }

    [Fact]
    public async Task GetPhoneNumberStatistics_ForNonExistentNumber_Returns404()
    {
        // Arrange
        var nonExistentNumber = "+1999999999";

        // Act
        Func<Task> act = async () => await GetPhoneNumberStatisticsAsync(nonExistentNumber);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(e => e.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecordMessageSent_WithSpecialCharacters_WorksCorrectly()
    {
        // Arrange
        var phoneNumber = "+1-800-555-1234";

        // Act
        await RecordMessageSentAsync(phoneNumber);
        var stats = await GetPhoneNumberStatisticsAsync(phoneNumber);

        // Assert
        stats.Should().NotBeNull();
        stats.PhoneNumber.Should().Be(phoneNumber);
        stats.TotalMessagesSent.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task RecordMessageSent_WithVeryLongPhoneNumber_WorksCorrectly()
    {
        // Arrange
        var phoneNumber = "+1234567890123456789012345678901234567890";

        // Act
        await RecordMessageSentAsync(phoneNumber);
        var stats = await GetPhoneNumberStatisticsAsync(phoneNumber);

        // Assert
        stats.Should().NotBeNull();
        stats.PhoneNumber.Should().Be(phoneNumber);
        stats.TotalMessagesSent.Should().BeGreaterThanOrEqualTo(1);
    }
} 