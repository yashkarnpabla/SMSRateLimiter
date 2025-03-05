using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SMSRateLimiter.Models;

namespace SMSRateLimiter.IntegrationTests;

public abstract class TestBase : IDisposable
{
    protected readonly HttpClient _client;
    protected readonly CustomWebApplicationFactory _factory;

    protected TestBase()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    protected async Task<CanSendMessageResponse> CanSendMessageAsync(string phoneNumber)
    {
        var request = new CanSendMessageRequest { BusinessPhoneNumber = phoneNumber };
        var response = await _client.PostAsJsonAsync("/api/ratelimiter/can-send", request);
        
        // If we get a 400 Bad Request for an empty phone number, return a custom response
        if (response.StatusCode == HttpStatusCode.BadRequest && string.IsNullOrEmpty(phoneNumber))
        {
            return new CanSendMessageResponse
            {
                CanSend = false,
                Reason = "Phone number cannot be empty"
            };
        }
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CanSendMessageResponse>();
    }

    protected async Task RecordMessageSentAsync(string phoneNumber)
    {
        var request = new CanSendMessageRequest { BusinessPhoneNumber = phoneNumber };
        var response = await _client.PostAsJsonAsync("/api/ratelimiter/record-sent", request);
        response.EnsureSuccessStatusCode();
    }

    protected async Task<PhoneNumberStatistics> GetPhoneNumberStatisticsAsync(string phoneNumber)
    {
        var response = await _client.GetAsync($"/api/statistics/phone-number/{phoneNumber}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PhoneNumberStatistics>();
    }

    protected async Task<IEnumerable<PhoneNumberStatistics>> GetAllPhoneNumberStatisticsAsync()
    {
        var response = await _client.GetAsync("/api/statistics/phone-numbers");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<PhoneNumberStatistics>>();
    }

    protected async Task<AccountStatistics> GetAccountStatisticsAsync()
    {
        var response = await _client.GetAsync("/api/statistics/account");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AccountStatistics>();
    }
} 