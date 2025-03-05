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

## Running the Project from GitHub

### Prerequisites
- .NET 9.0 SDK or later
- Git

### Clone and Run
1. Clone the repository:
   ```bash
   git clone https://github.com/YOUR_USERNAME/SMSRateLimiter.git
   cd SMSRateLimiter
   ```

2. Run the application:
   ```bash
   cd SMSRateLimiter
   dotnet run
   ```

3. Open a web browser and navigate to `http://localhost:5134` to access the dashboard.

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

## GitHub Actions for Continuous Integration (Optional)

You can also set up GitHub Actions to automatically run tests whenever changes are pushed to your repository. Create a file at `.github/workflows/dotnet.yml` with the following content:

```yaml
name: .NET

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

## Making Your Repository Stand Out

To make your repository more professional and user-friendly:

1. **Add Screenshots**: Include screenshots of your application in the README.md file to show what the dashboard looks like.

2. **Create a Demo Video**: Consider creating a short demo video showing how the application works and linking to it in your README.

3. **Add Badges**: Add badges to your README.md showing build status, test coverage, etc.

4. **Create a Wiki**: For more detailed documentation, consider creating a Wiki on GitHub.

5. **Add Issues and Project Board**: Set up issue templates and a project board to track future enhancements.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
