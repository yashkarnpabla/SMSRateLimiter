using System.Net; // Import for HTTP status codes
using FluentAssertions; // Import for fluent assertions to make tests more readable
using Xunit; // Import for xUnit testing framework

namespace SMSRateLimiter.IntegrationTests; // Define the namespace for integration tests

public class EdgeCaseTests : TestBase // Test class for edge cases, inherits from TestBase
{
    [Fact] // Attribute to mark this method as a test
    public async Task CanSendMessage_WithInvalidPhoneNumberFormat_StillWorks() // Test that verifies the system accepts invalid phone number formats
    {
        // Arrange
        var phoneNumber = "invalid-format"; // Define an invalid phone number format

        // Act
        var result = await CanSendMessageAsync(phoneNumber); // Call the API to check if a message can be sent

        // Assert
        result.Should().NotBeNull(); // Verify the result is not null
        result.CanSend.Should().BeTrue(); // Verify that the message can be sent despite invalid format
    }

    [Fact] // Attribute to mark this method as a test
    public async Task GetPhoneNumberStatistics_ForNonExistentNumber_Returns404() // Test that verifies getting statistics for a non-existent number returns 404
    {
        // Arrange
        var nonExistentNumber = "+1999999999"; // Define a phone number that hasn't been used

        // Act
        Func<Task> act = async () => await GetPhoneNumberStatisticsAsync(nonExistentNumber); // Create a function that calls the API

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>() // Verify that the function throws an HttpRequestException
            .Where(e => e.StatusCode == HttpStatusCode.NotFound); // Verify that the exception has a 404 Not Found status code
    }

    [Fact] // Attribute to mark this method as a test
    public async Task RecordMessageSent_WithSpecialCharacters_WorksCorrectly() // Test that verifies the system handles phone numbers with special characters
    {
        // Arrange
        var phoneNumber = "+1-800-555-1234"; // Define a phone number with hyphens

        // Act
        await RecordMessageSentAsync(phoneNumber); // Call the API to record a message as sent
        var stats = await GetPhoneNumberStatisticsAsync(phoneNumber); // Get statistics for the phone number

        // Assert
        stats.Should().NotBeNull(); // Verify statistics are not null
        stats.PhoneNumber.Should().Be(phoneNumber); // Verify the phone number matches exactly, including special characters
        stats.TotalMessagesSent.Should().BeGreaterThanOrEqualTo(1); // Verify at least one message was recorded
    }

    [Fact] // Attribute to mark this method as a test
    public async Task RecordMessageSent_WithVeryLongPhoneNumber_WorksCorrectly() // Test that verifies the system handles very long phone numbers
    {
        // Arrange
        var phoneNumber = "+1234567890123456789012345678901234567890"; // Define an unusually long phone number

        // Act
        await RecordMessageSentAsync(phoneNumber); // Call the API to record a message as sent
        var stats = await GetPhoneNumberStatisticsAsync(phoneNumber); // Get statistics for the phone number

        // Assert
        stats.Should().NotBeNull(); // Verify statistics are not null
        stats.PhoneNumber.Should().Be(phoneNumber); // Verify the phone number matches exactly
        stats.TotalMessagesSent.Should().BeGreaterThanOrEqualTo(1); // Verify at least one message was recorded
    }
}