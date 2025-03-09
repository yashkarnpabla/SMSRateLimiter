using Microsoft.AspNetCore.SignalR; // Import SignalR namespace for real-time communication
using SMSRateLimiter.Hubs; // Import Hubs namespace for the StatisticsHub

namespace SMSRateLimiter.Services; // Define the namespace for this service

/// <summary>
/// Background service that periodically broadcasts statistics to connected clients
/// </summary>
public class StatisticsBroadcastService : BackgroundService // Inherits from BackgroundService to run as a long-running service
{
    private readonly IServiceProvider _serviceProvider; // For creating scoped services during execution
    private readonly ILogger<StatisticsBroadcastService> _logger; // For logging information and errors
    private readonly TimeSpan _broadcastInterval = TimeSpan.FromSeconds(1); // Interval between broadcasts (1 second)

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsBroadcastService"/> class
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="logger">Logger</param>
    public StatisticsBroadcastService(IServiceProvider serviceProvider, ILogger<StatisticsBroadcastService> logger) // Constructor with dependency injection
    {
        _serviceProvider = serviceProvider; // Initialize the service provider
        _logger = logger; // Initialize the logger
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) // Main execution method that runs when the service starts
    {
        _logger.LogInformation("Statistics broadcast service is starting"); // Log service startup

        while (!stoppingToken.IsCancellationRequested) // Continue running until cancellation is requested
        {
            try
            {
                await BroadcastStatisticsAsync(); // Call method to broadcast statistics
            }
            catch (Exception ex) // Catch any exceptions to prevent the service from crashing
            {
                _logger.LogError(ex, "Error broadcasting statistics"); // Log the error
            }

            await Task.Delay(_broadcastInterval, stoppingToken); // Wait for the broadcast interval before the next broadcast
        }

        _logger.LogInformation("Statistics broadcast service is stopping"); // Log service shutdown
    }

    private async Task BroadcastStatisticsAsync() // Method to fetch and broadcast statistics
    {
        using var scope = _serviceProvider.CreateScope(); // Create a service scope for dependency resolution
        var rateLimiterService = scope.ServiceProvider.GetRequiredService<IRateLimiterService>(); // Get the rate limiter service
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<StatisticsHub>>(); // Get the SignalR hub context

        // Get account statistics
        var accountStats = await rateLimiterService.GetAccountStatisticsAsync(); // Fetch account-wide statistics
        await hubContext.Clients.All.SendAsync("ReceiveAccountStatistics", accountStats); // Broadcast account statistics to all clients

        // Get phone number statistics
        var phoneNumberStats = await rateLimiterService.GetAllPhoneNumberStatisticsAsync(); // Fetch statistics for all phone numbers
        await hubContext.Clients.All.SendAsync("ReceivePhoneNumberStatistics", phoneNumberStats); // Broadcast phone number statistics to all clients
    }
}