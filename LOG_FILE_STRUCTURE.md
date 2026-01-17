# Log File Structure

## Overview

The application uses Serilog with proper filtering to ensure each log file contains only relevant content for its module or process.

## Log Files

### API Service

#### `logs/api-host-{Date}.log`
**Contains:** API host process, startup, infrastructure, middleware, database operations

**Example content:**
- Application startup messages
- Database initialization
- Module registration
- Transaction middleware operations
- ASP.NET Core infrastructure (warnings only)
- gRPC infrastructure (warnings only)

**Excludes:** User, Team, and Issue module business logic

#### `logs/user-module-{Date}.log`
**Contains:** Only logs from `TicketSystem.User.*` namespace

**Example content:**
- User creation, updates, deletion
- User service operations
- User repository database calls
- User gRPC service operations

**Filter:** `StartsWith(SourceContext, 'TicketSystem.User')`

#### `logs/team-module-{Date}.log`
**Contains:** Only logs from `TicketSystem.Team.*` namespace

**Example content:**
- Team creation, updates
- Team member management
- Team service operations
- Team repository database calls
- Cross-module calls to User module

**Filter:** `StartsWith(SourceContext, 'TicketSystem.Team')`

#### `logs/issue-module-{Date}.log`
**Contains:** Only logs from `TicketSystem.Issue.*` namespace

**Example content:**
- Issue creation, updates, deletion
- Issue assignment operations
- Issue status transitions
- Issue repository database calls
- Cross-module calls to User and Team modules

**Filter:** `StartsWith(SourceContext, 'TicketSystem.Issue')`

### Client Application

#### `logs/client-{Date}.log`
**Contains:** All client application logs

**Example content:**
- Client startup
- gRPC client operations
- User interface interactions
- Client-side error handling

**Note:** gRPC client infrastructure logging (connection management, etc.) is suppressed to Warning level.

## Filtering Mechanism

The filtering uses Serilog's sub-logger configuration with `Serilog.Filters.Expressions`:

### Module Logs (Inclusion Filter)
```json
{
  "Name": "Logger",
  "Args": {
    "configureLogger": {
      "Filter": [
        {
          "Name": "ByIncludingOnly",
          "Args": {
            "expression": "StartsWith(SourceContext, 'TicketSystem.ModuleName')"
          }
        }
      ]
    }
  }
}
```

### API Host Log (Exclusion Filter)
```json
{
  "Name": "Logger",
  "Args": {
    "configureLogger": {
      "Filter": [
        {
          "Name": "ByExcluding",
          "Args": {
            "expression": "StartsWith(SourceContext, 'TicketSystem.User') or StartsWith(SourceContext, 'TicketSystem.Team') or StartsWith(SourceContext, 'TicketSystem.Issue')"
          }
        }
      ]
    }
  }
}
```

## Noise Reduction

The following infrastructure components are set to `Warning` level to reduce log noise:

- `Microsoft.AspNetCore.*` - ASP.NET Core framework
- `Grpc.AspNetCore.Server` - gRPC server infrastructure
- `Grpc.Net.Client` - gRPC client infrastructure
- `Microsoft.Extensions.Http` - HTTP client infrastructure (production)

This ensures log files contain primarily business logic and application-specific information.

## Verification

To verify the filtering is working correctly:

1. Start the application
2. Perform operations (create user, create team, create issue)
3. Check each log file:
   - `api-host-*.log` should contain startup and infrastructure logs
   - `user-module-*.log` should contain ONLY user-related logs
   - `team-module-*.log` should contain ONLY team-related logs
   - `issue-module-*.log` should contain ONLY issue-related logs

## Development vs Production

**Development (`appsettings.Development.json`):**
- Console output only
- More verbose logging (Debug level)
- Useful for immediate feedback during development

**Production (`appsettings.json`):**
- File output with filtering
- Standard logging (Information level)
- Separated by module for easier troubleshooting
