# SMS Rate Limiter

A microservice in .NET Core (C#) that acts as a gatekeeper for SMS messages, ensuring that rate limits are respected before sending messages to an external provider.

## Features

- Enforces rate limits for individual business phone numbers (5 messages per second)
- Enforces account-wide rate limits (20 messages per second)
- Provides real-time statistics via a modern web dashboard with visual rate limit indicators
- Automatically cleans up tracking data for inactive phone numbers
- RESTful API for integration with other services
- Real-time updates via SignalR

## Rate Limits

The service enforces two specific limits:

1. A maximum of 5 messages can be sent from a single business phone number per second
2. A maximum of 20 messages can be sent across the entire account per second

These limits are configurable in the `appsettings.json` file.

## API Endpoints

### Rate Limiter

- `POST /api/ratelimiter/can-send` - Check if a message can be sent from a given business phone number
- `POST /api/ratelimiter/record-sent` - Record that a message was sent from a given business phone number

### Statistics

- `GET /api/statistics/account` - Get overall account statistics
- `GET /api/statistics/phone-numbers` - Get statistics for all phone numbers
- `GET /api/statistics/phone-number/{phoneNumber}` - Get statistics for a specific phone number

## Web Dashboard

The service includes a modern web dashboard for monitoring the rate limits and statistics in real-time. The dashboard is available at the root URL of the service.

### Dashboard Features

- Account-wide statistics (messages per second, total messages, active phone numbers)
- Per-phone number statistics with visual rate limit indicators
- Dedicated Rate Limits tab with detailed information
- Phone number filtering
- Real-time updates via SignalR
- Modern glass morphism UI design

## Project Structure

- `SMSRateLimiter` - Main application project
- `SMSRateLimiter.IntegrationTests` - Integration tests for the application

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later

### Running the Service

1. Clone the repository
2. Navigate to the project directory
3. Run the service:

```bash
cd SMSRateLimiter
dotnet run
```

4. Open a web browser and navigate to `http://localhost:5134` to access the dashboard

### Running the Tests

To run all tests in the solution:

```bash
dotnet test
```

To run only the integration tests:

```bash
cd SMSRateLimiter.IntegrationTests
dotnet test
```

## Configuration

The service can be configured by modifying the `appsettings.json` file:

```json
{
  "RateLimitConfig": {
    "MaxMessagesPerNumberPerSecond": 5,
    "MaxMessagesPerAccountPerSecond": 20,
    "CleanupInactiveNumbersAfterMinutes": 60
  }
}
```

## License

This project is licensed under the MIT License - see the LICENSE file for details. 