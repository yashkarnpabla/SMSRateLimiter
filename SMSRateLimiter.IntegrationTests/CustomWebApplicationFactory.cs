using Microsoft.AspNetCore.Mvc.Testing; // Import for WebApplicationFactory to create test server

namespace SMSRateLimiter.IntegrationTests; // Define the namespace for integration tests

public class CustomWebApplicationFactory : WebApplicationFactory<Program> // Factory for creating a test server with the application
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder) // Method to configure the test web host
    {
        // You can configure the test server here if needed
        base.ConfigureWebHost(builder); // Call the base implementation to set up the default configuration
    }
}