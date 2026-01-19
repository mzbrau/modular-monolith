# TicketSystem Modular Monolith Application

This application is a modular monolith for managing tickets. It consists of three modules: **Issue**, **Team**, and **User**.

## Database

Settings for the shared database connection and behavior.

### DatabaseTimeoutSeconds

The timeout in seconds for database operations. This ensures requests don't hang indefinitely if the database is unresponsive.

### EnableDatabaseLogging

When enabled, detailed SQL queries and parameters will be logged to the console. **Warning:** This may log sensitive data and should be disabled in production.

## API

Settings aimed at controlling the API behavior and performance.

### ApiTimeoutSeconds

The maximum time allowed for an API request to complete before it is cancelled.

### RateLimitPerMinute

The maximum number of requests a single client IP can make per minute. This protects the service from abuse.

## Issue

Configuration for the Issue module.

### MinTitleLength

The minimum number of characters required for an issue title.

### MaxTitleLength

The maximum number of characters allowed for an issue title.

### Issue MaxDescriptionLength

The maximum number of characters allowed for the issue description.

### DueDateWarningDays

The number of days before an issue's due date to trigger a warning notification.

### AllowNoDueDate

Determines if issues can be created without a specific due date.

### DefaultPriority

The priority assigned to new issues if one is not specified. Valid values are: Low, Medium, High, Critical.

### AutoResolveOnComplete

If true, issues will automatically transition to the Resolved status when all tasks are marked as complete.

## Team

Configuration for the Team module.

### MinNameLength

The minimum length for a team name.

### MaxNameLength

The maximum length for a team name.

### Team MaxDescriptionLength

The maximum length for a team description.

### MaxTeamMembers

The hard limit on how many users can be assigned to a single team.

### AllowEmptyTeams

If true, teams can be created without any members initially.

### MinTeamMembers

The minimum number of members required for a team (if empty teams are not allowed).

### RequireTeamLead

If true, every team must have at least one user designated as a team lead.

## User

Configuration for the User module.

### MinFirstNameLength

Minimum length for a user's first name.

### MaxFirstNameLength

Maximum length for a user's first name.

### MinLastNameLength

Minimum length for a user's last name.

### MaxLastNameLength

Maximum length for a user's last name.

### AllowedEmailDomains

A comma-separated list of allowed email domains (e.g., "company.com, specialized.org"). If empty, all domains are allowed.

### RequireEmailVerification

If true, users must verify their email address before their account becomes active.

### AutoDeactivateAfterDays

The number of days of inactivity after which a user account is automatically deactivated. Set to 0 to disable this feature.

### AllowPermanentDelete

If true, user records can be permanently deleted from the database. If false, "deletion" only soft-deletes or deactivates the user.
