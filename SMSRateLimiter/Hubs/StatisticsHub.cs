using Microsoft.AspNetCore.SignalR; // Import SignalR namespace for real-time communication
using SMSRateLimiter.Models; // Import Models namespace for statistics data types

namespace SMSRateLimiter.Hubs; // Define the namespace for this hub

/// <summary>
/// SignalR hub for real-time statistics updates
/// </summary>
public class StatisticsHub : Hub // Inherit from Hub class to enable real-time communication
{
    /// <summary>
    /// Sends account statistics to all connected clients
    /// </summary>
    /// <param name="statistics">Account statistics</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public async Task SendAccountStatistics(AccountStatistics statistics) // Method to broadcast account statistics
    {
        await Clients.All.SendAsync("ReceiveAccountStatistics", statistics); // Send to all connected clients using the "ReceiveAccountStatistics" event name
    }

    /// <summary>
    /// Sends phone number statistics to all connected clients
    /// </summary>
    /// <param name="statistics">Phone number statistics</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public async Task SendPhoneNumberStatistics(PhoneNumberStatistics statistics) // Method to broadcast phone number statistics
    {
        await Clients.All.SendAsync("ReceivePhoneNumberStatistics", statistics); // Send to all connected clients using the "ReceivePhoneNumberStatistics" event name
    }
}