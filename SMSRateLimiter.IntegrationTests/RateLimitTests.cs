using FluentAssertions; // Import for fluent assertions to make tests more readable
using Xunit; // Import for xUnit testing framework

namespace SMSRateLimiter.IntegrationTests; // Define the namespace for integration tests

public class RateLimitTests : TestBase // Test class for rate limiting functionality, inherits from TestBase
{
    [Fact] // Attribute to mark this method as a test
    public async Task CanSendMessage_ExceedingPhoneNumberLimit_ReturnsFalse() // Test that verifies rate limiting for a single phone number
    {
        // Arrange
        var phoneNumber = "+1333333333"; // Define a test phone number
        
        // Send 5 messages in quick succession to hit the per-phone number limit
        for (int i = 0; i < 5; i++) // Loop to send 5 messages (default limit per phone number)
        {
            await RecordMessageSentAsync(phoneNumber); // Record each message as sent
        }

        // Act
        var result = await CanSendMessageAsync(phoneNumber); // Try to send another message after hitting the limit

        // Assert
        result.Should().NotBeNull(); // Verify the result is not null
        result.CanSend.Should().BeFalse(); // Verify that the message cannot be sent due to rate limiting
        result.Reason.Should().Contain("Rate limit exceeded for phone number"); // Verify the reason mentions the phone number rate limit
    }

    [Fact] // Attribute to mark this method as a test
    public async Task CanSendMessage_ExceedingAccountLimit_ReturnsFalse() // Test that verifies account-wide rate limiting
    {
        // Arrange
        // Use different phone numbers to avoid hitting the per-number limit
        var phoneNumbers = Enumerable.Range(1, 20) // Generate 20 different phone numbers (default account limit is 20)
            .Select(i => $"+1{i.ToString().PadLeft(10, '0')}") // Format each number as a phone number
            .ToArray();

        // Send messages from 20 different phone numbers to hit the account-wide limit
        foreach (var phoneNumber in phoneNumbers) // Loop through all phone numbers
        {
            await RecordMessageSentAsync(phoneNumber); // Record a message as sent from each phone number
        }

        // Act
        var result = await CanSendMessageAsync("+1999999999"); // Try to send another message from a new number after hitting the account limit

        // Assert
        result.Should().NotBeNull(); // Verify the result is not null
        result.CanSend.Should().BeFalse(); // Verify that the message cannot be sent due to rate limiting
        result.Reason.Should().Contain("Account-wide rate limit exceeded"); // Verify the reason mentions the account-wide rate limit
    }
}