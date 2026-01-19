using Fig.Client;
using Fig.Client.Abstractions.Attributes;
using Fig.Client.Abstractions.Enums;
using TicketSystem.Issue.Application.Configuration;
using TicketSystem.Team.Application.Configuration;
using TicketSystem.User.Application.Configuration;

namespace TicketSystem.Api.Configuration;

public class TicketSystemSettings : SettingsBase
{
    private const string MarkdownKey = "$TicketSystemSettings";
    public override string ClientDescription => MarkdownKey;

    [NestedSetting]
    public IssueSettings Issue { get; set; } = new();

    [NestedSetting]
    public TeamSettings Team { get; set; } = new();

    [NestedSetting]
    public UserSettings User { get; set; } = new();

    [Fig.Client.Abstractions.Attributes.Setting($"{MarkdownKey}#DatabaseTimeoutSeconds")]
    [Category("Database", "#764BA2")]
    [ValidateIsBetween(5, 300, Inclusion.Inclusive)]
    public int DatabaseTimeoutSeconds { get; set; } = 30;

    [Fig.Client.Abstractions.Attributes.Setting($"{MarkdownKey}#EnableDatabaseLogging")]
    [Category("Database", "#764BA2")]
    public bool EnableDatabaseLogging { get; set; } = false;

    [Fig.Client.Abstractions.Attributes.Setting($"{MarkdownKey}#ApiTimeoutSeconds")]
    [Category("API", "#F093FB")]
    [ValidateIsBetween(10, 600, Inclusion.Inclusive)]
    public int ApiTimeoutSeconds { get; set; } = 60;

    [Fig.Client.Abstractions.Attributes.Setting($"{MarkdownKey}#RateLimitPerMinute")]
    [Category("API", "#F093FB")]
    [ValidateIsBetween(10, 10000, Inclusion.Inclusive)]
    public int RateLimitPerMinute { get; set; } = 1000;

    public override IEnumerable<string> GetValidationErrors()
    {
        // Cross-validation: Title min must be less than max
        if (Issue.MinTitleLength >= Issue.MaxTitleLength)
        {
            yield return "Issue MinTitleLength must be less than MaxTitleLength";
        }

        // Cross-validation: First name min must be less than max
        if (User.MinFirstNameLength >= User.MaxFirstNameLength)
        {
            yield return "User MinFirstNameLength must be less than MaxFirstNameLength";
        }

        // Cross-validation: Last name min must be less than max
        if (User.MinLastNameLength >= User.MaxLastNameLength)
        {
            yield return "User MinLastNameLength must be less than MaxLastNameLength";
        }

        // Cross-validation: Team name min must be less than max
        if (Team.MinNameLength >= Team.MaxNameLength)
        {
            yield return "Team MinNameLength must be less than MaxNameLength";
        }

        // Cross-validation: Min team members must be achievable if not allowing empty teams
        if (Team is { AllowEmptyTeams: false, MinTeamMembers: 0 })
        {
            yield return "Team MinTeamMembers must be at least 1 when AllowEmptyTeams is false";
        }
    }
}
