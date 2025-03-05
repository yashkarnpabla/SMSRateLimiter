using Microsoft.AspNetCore.Mvc.Testing;

namespace SMSRateLimiter.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        // You can configure the test server here if needed
        base.ConfigureWebHost(builder);
    }
} 