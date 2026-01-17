# Logging Configuration with Serilog

This solution uses Serilog for structured logging with separate log files for each module and configurable log levels.

## Architecture

The logging infrastructure is centralized in the `TicketSystem.ServiceDefaults` project to maintain the modular monolith architecture. This ensures:
- No direct dependencies between modules for logging
- Centralized configuration in a shared infrastructure component
- Consistent logging patterns across all projects

## Configuration

### Log Files

Logs are organized into separate files by module with proper filtering:

**API Service:**

- `logs/api-host-{Date}.log` - API host process logs (startup, middleware, database initialization)
- `logs/user-module-{Date}.log` - User module specific logs only
- `logs/team-module-{Date}.log` - Team module specific logs only
- `logs/issue-module-{Date}.log` - Issue module specific logs only

**Client Service:**

- `logs/client-{Date}.log` - Client application logs

Each log file:

- Rolls daily (based on `rollingInterval: Day`)
- Retains 7 days of logs (based on `retainedFileCountLimit: 7`)
- Uses structured output format with timestamp, level, source context, message, and exception
- Contains only logs from its specific namespace (properly filtered using Serilog sub-loggers)

### Log Levels

Log levels can be configured independently for each module in `appsettings.json`:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft.AspNetCore": "Warning",
      "Grpc.AspNetCore.Server": "Warning",
      "Grpc.Net.Client": "Warning",
      "TicketSystem.User": "Information",
      "TicketSystem.Team": "Information",
      "TicketSystem.Issue": "Information"
    }
  }
}
```

Available log levels (from most to least verbose):

- `Verbose` - Very detailed logs, typically only enabled for troubleshooting
- `Debug` - Detailed information for debugging purposes
- `Information` - General informational messages (default)
- `Warning` - Warning messages for non-critical issues
- `Error` - Error messages for failures
- `Fatal` - Critical errors causing application shutdown

**Note:** gRPC and ASP.NET Core infrastructure logging is set to `Warning` by default to reduce noise.

### Environment-Specific Configuration

**Production (`appsettings.json`):**
- Default level: `Information`
- Writes to file only
- Module-specific filtering to separate log files

**Development (`appsettings.Development.json`):**
- Default level: `Debug`
- Writes to console for immediate feedback
- More verbose logging for modules

### Module Filtering

Module-specific logs are filtered using Serilog sub-loggers with expression-based filters:

```json
{
  "Name": "Logger",
  "Args": {
    "configureLogger": {
      "Filter": [
        {
          "Name": "ByIncludingOnly",
          "Args": {
            "expression": "StartsWith(SourceContext, 'TicketSystem.User')"
          }
        }
      ],
      "WriteTo": [
        {
          "Name": "File",
          "Args": {
            "path": "logs/user-module-.log",
            "rollingInterval": "Day"
          }
        }
      ]
    }
  }
}
```

This configuration creates a sub-logger that:

1. Filters logs to include only those from the specified namespace
2. Writes those logs to a dedicated file
3. Ensures each module's logs are isolated

The API host log file uses an exclusion filter to capture everything except module-specific logs:

```json
{
  "Name": "ByExcluding",
  "Args": {
    "expression": "StartsWith(SourceContext, 'TicketSystem.User') or StartsWith(SourceContext, 'TicketSystem.Team') or StartsWith(SourceContext, 'TicketSystem.Issue')"
  }
}
```

## Usage

### In Application Code

Use the standard `ILogger<T>` dependency injection:

```csharp
public class UserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public void SomeMethod()
    {
        _logger.LogInformation("Processing user operation");
        _logger.LogWarning("User {UserId} validation warning", userId);
        _logger.LogError(ex, "Error processing user {UserId}", userId);
    }
}
```

### Structured Logging

Serilog supports structured logging with message templates:

```csharp
// Good - structured with named properties
_logger.LogInformation("User {UserId} created team {TeamId}", userId, teamId);

// Avoid - string interpolation loses structure
_logger.LogInformation($"User {userId} created team {teamId}");
```

### Log Context Enrichment

Logs are automatically enriched with:
- `ApplicationName` - The name of the application (API or Client)
- `SourceContext` - The full type name of the logger source

## Customization

### Changing Log Levels Per Module

Edit `appsettings.json` or `appsettings.Development.json`:

```json
"Override": {
  "TicketSystem.User": "Debug",      // More verbose for User module
  "TicketSystem.Team": "Warning",    // Only warnings and above for Team
  "TicketSystem.Issue": "Information" // Standard level for Issue
}
```

### Adding New Modules

When adding a new module, update the API's `appsettings.json` to include:

1. Override for minimum level:

```json
"Override": {
  "TicketSystem.NewModule": "Information"
}
```

2. New sub-logger with filter:

```json
{
  "Name": "Logger",
  "Args": {
    "configureLogger": {
      "Filter": [
        {
          "Name": "ByIncludingOnly",
          "Args": {
            "expression": "StartsWith(SourceContext, 'TicketSystem.NewModule')"
          }
        }
      ],
      "WriteTo": [
        {
          "Name": "File",
          "Args": {
            "path": "logs/new-module-.log",
            "rollingInterval": "Day",
            "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
            "retainedFileCountLimit": 7
          }
        }
      ]
    }
  }
}
```

3. Update the API host log exclusion filter:

```json
"expression": "StartsWith(SourceContext, 'TicketSystem.User') or StartsWith(SourceContext, 'TicketSystem.Team') or StartsWith(SourceContext, 'TicketSystem.Issue') or StartsWith(SourceContext, 'TicketSystem.NewModule')"
```

## Architecture Compliance

The logging implementation respects the modular monolith architecture:

✅ **No Module Dependencies**: Modules don't reference each other for logging
✅ **Centralized in ServiceDefaults**: Logging configuration is in shared infrastructure
✅ **Module Isolation**: Each module logs independently with its own configuration
✅ **Clean Boundaries**: Architecture tests verify no improper dependencies introduced

## Packages Used

- `Serilog.AspNetCore` - ASP.NET Core integration
- `Serilog.Sinks.File` - File output sink
- `Serilog.Sinks.Console` - Console output sink (development)
- `Serilog.Filters.Expressions` - Expression-based filtering for module separation
