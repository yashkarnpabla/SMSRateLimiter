using Microsoft.AspNetCore.SignalR;
using SMSRateLimiter.Hubs;

namespace SMSRateLimiter.Services;

/// <summary>
/// Background service that periodically broadcasts statistics to connected clients
/// </summary>
public class StatisticsBroadcastService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StatisticsBroadcastService> _logger;
    private readonly TimeSpan _broadcastInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsBroadcastService"/> class
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="logger">Logger</param>
    public StatisticsBroadcastService(IServiceProvider serviceProvider, ILogger<StatisticsBroadcastService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Statistics broadcast service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await BroadcastStatisticsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting statistics");
            }

            await Task.Delay(_broadcastInterval, stoppingToken);
        }

        _logger.LogInformation("Statistics broadcast service is stopping");
    }

    private async Task BroadcastStatisticsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var rateLimiterService = scope.ServiceProvider.GetRequiredService<IRateLimiterService>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<StatisticsHub>>();

        // Get account statistics
        var accountStats = await rateLimiterService.GetAccountStatisticsAsync();
        await hubContext.Clients.All.SendAsync("ReceiveAccountStatistics", accountStats);

        // Get phone number statistics
        var phoneNumberStats = await rateLimiterService.GetAllPhoneNumberStatisticsAsync();
        await hubContext.Clients.All.SendAsync("ReceivePhoneNumberStatistics", phoneNumberStats);
    }
} 