using Microsoft.AspNetCore.SignalR;
using SMSRateLimiter.Models;

namespace SMSRateLimiter.Hubs;

/// <summary>
/// SignalR hub for real-time statistics updates
/// </summary>
public class StatisticsHub : Hub
{
    /// <summary>
    /// Sends account statistics to all connected clients
    /// </summary>
    /// <param name="statistics">Account statistics</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public async Task SendAccountStatistics(AccountStatistics statistics)
    {
        await Clients.All.SendAsync("ReceiveAccountStatistics", statistics);
    }

    /// <summary>
    /// Sends phone number statistics to all connected clients
    /// </summary>
    /// <param name="statistics">Phone number statistics</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public async Task SendPhoneNumberStatistics(PhoneNumberStatistics statistics)
    {
        await Clients.All.SendAsync("ReceivePhoneNumberStatistics", statistics);
    }
} 