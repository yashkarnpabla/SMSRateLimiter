using SMSRateLimiter.Hubs;
using SMSRateLimiter.Models;
using SMSRateLimiter.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

// Configure rate limit settings
builder.Services.Configure<RateLimitConfig>(builder.Configuration.GetSection("RateLimitConfig"));

// Register the rate limiter service as a singleton
builder.Services.AddSingleton<IRateLimiterService, RateLimiterService>();

// Add SignalR
builder.Services.AddSignalR();

// Add the statistics broadcast service
builder.Services.AddHostedService<StatisticsBroadcastService>();

// Add CORS for the web interface
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SMS Rate Limiter API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable serving static files and set default file
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();
app.MapHub<StatisticsHub>("/hubs/statistics");

app.Run();
