using FluentAssertions; // Import for fluent assertions to make tests more readable
using Xunit; // Import for xUnit testing framework

namespace SMSRateLimiter.IntegrationTests; // Define the namespace for integration tests

public class BasicFunctionalityTests : TestBase // Test class for basic functionality, inherits from TestBase
{
    [Fact] // Attribute to mark this method as a test
    public async Task CanSendMessage_WithValidPhoneNumber_ReturnsTrue() // Test that verifies a valid phone number can send a message
    {
        // Arrange
        var phoneNumber = "+1234567890"; // Define a valid phone number for testing

        // Act
        var result = await CanSendMessageAsync(phoneNumber); // Call the API to check if a message can be sent

        // Assert
        result.Should().NotBeNull(); // Verify the result is not null
        result.CanSend.Should().BeTrue(); // Verify that the message can be sent
        result.Reason.Should().BeNull(); // Verify there's no reason preventing the message from being sent
    }

    [Fact] // Attribute to mark this method as a test
    public async Task CanSendMessage_WithEmptyPhoneNumber_ReturnsFalse() // Test that verifies an empty phone number cannot send a message
    {
        // Arrange
        var phoneNumber = string.Empty; // Define an empty phone number for testing

        // Act
        var result = await CanSendMessageAsync(phoneNumber); // Call the API to check if a message can be sent

        // Assert
        result.Should().NotBeNull(); // Verify the result is not null
        result.CanSend.Should().BeFalse(); // Verify that the message cannot be sent
        result.Reason.Should().NotBeNullOrEmpty(); // Verify there's a reason provided for why the message cannot be sent
    }

    [Fact] // Attribute to mark this method as a test
    public async Task RecordMessageSent_WithValidPhoneNumber_UpdatesStatistics() // Test that verifies recording a message updates statistics
    {
        // Arrange
        var phoneNumber = "+1987654321"; // Define a valid phone number for testing

        // Act
        await RecordMessageSentAsync(phoneNumber); // Call the API to record a message as sent
        var phoneStats = await GetPhoneNumberStatisticsAsync(phoneNumber); // Get statistics for the phone number
        var accountStats = await GetAccountStatisticsAsync(); // Get account-wide statistics

        // Assert
        phoneStats.Should().NotBeNull(); // Verify phone statistics are not null
        phoneStats.PhoneNumber.Should().Be(phoneNumber); // Verify the phone number matches
        phoneStats.TotalMessagesSent.Should().BeGreaterThanOrEqualTo(1); // Verify at least one message was recorded
        
        accountStats.Should().NotBeNull(); // Verify account statistics are not null
        accountStats.TotalMessagesSent.Should().BeGreaterThanOrEqualTo(1); // Verify at least one message was recorded account-wide
        accountStats.ActivePhoneNumbers.Should().BeGreaterThanOrEqualTo(1); // Verify at least one phone number is active
    }

    [Fact] // Attribute to mark this method as a test
    public async Task GetAllPhoneNumberStatistics_ReturnsAllActivePhoneNumbers() // Test that verifies getting all phone number statistics returns all active numbers
    {
        // Arrange
        var phoneNumber1 = "+1111111111"; // Define first test phone number
        var phoneNumber2 = "+1222222222"; // Define second test phone number
        
        await RecordMessageSentAsync(phoneNumber1); // Record a message from the first phone number
        await RecordMessageSentAsync(phoneNumber2); // Record a message from the second phone number

        // Act
        var allStats = await GetAllPhoneNumberStatisticsAsync(); // Get statistics for all phone numbers

        // Assert
        allStats.Should().NotBeNull(); // Verify the statistics are not null
        allStats.Should().Contain(s => s.PhoneNumber == phoneNumber1); // Verify the first phone number is included
        allStats.Should().Contain(s => s.PhoneNumber == phoneNumber2); // Verify the second phone number is included
    }
}