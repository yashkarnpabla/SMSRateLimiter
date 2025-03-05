using FluentAssertions;
using Xunit;

namespace SMSRateLimiter.IntegrationTests;

public class BasicFunctionalityTests : TestBase
{
    [Fact]
    public async Task CanSendMessage_WithValidPhoneNumber_ReturnsTrue()
    {
        // Arrange
        var phoneNumber = "+1234567890";

        // Act
        var result = await CanSendMessageAsync(phoneNumber);

        // Assert
        result.Should().NotBeNull();
        result.CanSend.Should().BeTrue();
        result.Reason.Should().BeNull();
    }

    [Fact]
    public async Task CanSendMessage_WithEmptyPhoneNumber_ReturnsFalse()
    {
        // Arrange
        var phoneNumber = string.Empty;

        // Act
        var result = await CanSendMessageAsync(phoneNumber);

        // Assert
        result.Should().NotBeNull();
        result.CanSend.Should().BeFalse();
        result.Reason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RecordMessageSent_WithValidPhoneNumber_UpdatesStatistics()
    {
        // Arrange
        var phoneNumber = "+1987654321";

        // Act
        await RecordMessageSentAsync(phoneNumber);
        var phoneStats = await GetPhoneNumberStatisticsAsync(phoneNumber);
        var accountStats = await GetAccountStatisticsAsync();

        // Assert
        phoneStats.Should().NotBeNull();
        phoneStats.PhoneNumber.Should().Be(phoneNumber);
        phoneStats.TotalMessagesSent.Should().BeGreaterThanOrEqualTo(1);
        
        accountStats.Should().NotBeNull();
        accountStats.TotalMessagesSent.Should().BeGreaterThanOrEqualTo(1);
        accountStats.ActivePhoneNumbers.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetAllPhoneNumberStatistics_ReturnsAllActivePhoneNumbers()
    {
        // Arrange
        var phoneNumber1 = "+1111111111";
        var phoneNumber2 = "+1222222222";
        
        await RecordMessageSentAsync(phoneNumber1);
        await RecordMessageSentAsync(phoneNumber2);

        // Act
        var allStats = await GetAllPhoneNumberStatisticsAsync();

        // Assert
        allStats.Should().NotBeNull();
        allStats.Should().Contain(s => s.PhoneNumber == phoneNumber1);
        allStats.Should().Contain(s => s.PhoneNumber == phoneNumber2);
    }
} 