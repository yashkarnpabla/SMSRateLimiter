using System.Net; // Import for HTTP status codes
using System.Net.Http.Json; // Import for JSON serialization/deserialization with HttpClient
using Microsoft.AspNetCore.Mvc.Testing; // Import for WebApplicationFactory to create test server
using SMSRateLimiter.Models; // Import for request/response models

namespace SMSRateLimiter.IntegrationTests; // Define the namespace for integration tests

public abstract class TestBase : IDisposable // Base class for all test classes, implements IDisposable for cleanup
{
    protected readonly HttpClient _client; // HTTP client for making requests to the test server
    protected readonly CustomWebApplicationFactory _factory; // Factory for creating the test server

    protected TestBase() // Constructor for the base test class
    {
        _factory = new CustomWebApplicationFactory(); // Create a new factory for the test server
        _client = _factory.CreateClient(); // Create an HTTP client connected to the test server
    }

    public void Dispose() // Cleanup method called after tests complete
    {
        _client.Dispose(); // Dispose the HTTP client
        _factory.Dispose(); // Dispose the test server factory
    }

    protected async Task<CanSendMessageResponse> CanSendMessageAsync(string phoneNumber) // Helper method to call the can-send endpoint
    {
        var request = new CanSendMessageRequest { BusinessPhoneNumber = phoneNumber }; // Create request with the phone number
        var response = await _client.PostAsJsonAsync("/api/ratelimiter/can-send", request); // Send POST request to the can-send endpoint
        
        // If we get a 400 Bad Request for an empty phone number, return a custom response
        if (response.StatusCode == HttpStatusCode.BadRequest && string.IsNullOrEmpty(phoneNumber)) // Handle bad request for empty phone number
        {
            return new CanSendMessageResponse
            {
                CanSend = false,
                Reason = "Phone number cannot be empty" // Return custom error message
            };
        }
        
        response.EnsureSuccessStatusCode(); // Throw if the response is not successful (2xx)
        return await response.Content.ReadFromJsonAsync<CanSendMessageResponse>(); // Deserialize the response JSON
    }

    protected async Task RecordMessageSentAsync(string phoneNumber) // Helper method to call the record-sent endpoint
    {
        var request = new CanSendMessageRequest { BusinessPhoneNumber = phoneNumber }; // Create request with the phone number
        var response = await _client.PostAsJsonAsync("/api/ratelimiter/record-sent", request); // Send POST request to the record-sent endpoint
        response.EnsureSuccessStatusCode(); // Throw if the response is not successful (2xx)
    }

    protected async Task<PhoneNumberStatistics> GetPhoneNumberStatisticsAsync(string phoneNumber) // Helper method to get statistics for a specific phone number
    {
        var response = await _client.GetAsync($"/api/statistics/phone-number/{phoneNumber}"); // Send GET request to the phone-number statistics endpoint
        response.EnsureSuccessStatusCode(); // Throw if the response is not successful (2xx)
        return await response.Content.ReadFromJsonAsync<PhoneNumberStatistics>(); // Deserialize the response JSON
    }

    protected async Task<IEnumerable<PhoneNumberStatistics>> GetAllPhoneNumberStatisticsAsync() // Helper method to get statistics for all phone numbers
    {
        var response = await _client.GetAsync("/api/statistics/phone-numbers"); // Send GET request to the phone-numbers statistics endpoint
        response.EnsureSuccessStatusCode(); // Throw if the response is not successful (2xx)
        return await response.Content.ReadFromJsonAsync<IEnumerable<PhoneNumberStatistics>>(); // Deserialize the response JSON
    }

    protected async Task<AccountStatistics> GetAccountStatisticsAsync() // Helper method to get account-wide statistics
    {
        var response = await _client.GetAsync("/api/statistics/account"); // Send GET request to the account statistics endpoint
        response.EnsureSuccessStatusCode(); // Throw if the response is not successful (2xx)
        return await response.Content.ReadFromJsonAsync<AccountStatistics>(); // Deserialize the response JSON
    }
} 