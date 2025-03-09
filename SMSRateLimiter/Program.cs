using SMSRateLimiter.Hubs; // Import Hubs namespace for SignalR hub classes
using SMSRateLimiter.Models; // Import Models namespace for configuration and data types
using SMSRateLimiter.Services; // Import Services namespace for service interfaces and implementations

var builder = WebApplication.CreateBuilder(args); // Create a new web application builder with command-line arguments

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers(); // Add controller services to the dependency injection container

// Configure rate limit settings
builder.Services.Configure<RateLimitConfig>(builder.Configuration.GetSection("RateLimitConfig")); // Bind configuration from appsettings.json to RateLimitConfig class

// Register the rate limiter service as a singleton
builder.Services.AddSingleton<IRateLimiterService, RateLimiterService>(); // Register IRateLimiterService interface with RateLimiterService implementation as a singleton

// Add SignalR
builder.Services.AddSignalR(); // Add SignalR services to enable real-time communication

// Add the statistics broadcast service
builder.Services.AddHostedService<StatisticsBroadcastService>(); // Register StatisticsBroadcastService as a hosted service that runs in the background

// Add CORS for the web interface
builder.Services.AddCors(options => // Configure Cross-Origin Resource Sharing to allow web clients to access the API
{
    options.AddPolicy("AllowAll", policy => // Define a CORS policy named "AllowAll"
    {
        policy.AllowAnyOrigin() // Allow requests from any origin
              .AllowAnyMethod() // Allow any HTTP method (GET, POST, etc.)
              .AllowAnyHeader(); // Allow any HTTP headers
    });
});

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer(); // Add services to generate OpenAPI document for API endpoints
builder.Services.AddSwaggerGen(c => // Configure Swagger generation
{
    c.SwaggerDoc("v1", new() { Title = "SMS Rate Limiter API", Version = "v1" }); // Define API document info
});

var app = builder.Build(); // Build the web application

// Configure the HTTP request pipeline.
// Make Swagger available in all environments
app.UseSwagger(); // Enable Swagger middleware to generate the OpenAPI document
app.UseSwaggerUI(); // Enable Swagger UI middleware to provide a web UI for exploring the API

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS for secure communication

// Enable serving static files and set default file
app.UseDefaultFiles(); // Enable default file mapping (index.html, default.html, etc.)
app.UseStaticFiles(); // Enable serving static files from wwwroot folder

app.UseCors("AllowAll"); // Apply the "AllowAll" CORS policy to enable cross-origin requests

app.UseAuthorization(); // Enable authorization middleware

app.MapControllers(); // Map controller endpoints to routes
app.MapHub<StatisticsHub>("/hubs/statistics"); // Map the StatisticsHub to the "/hubs/statistics" endpoint for SignalR connections

app.Run(); // Start the web application and begin listening for requests
