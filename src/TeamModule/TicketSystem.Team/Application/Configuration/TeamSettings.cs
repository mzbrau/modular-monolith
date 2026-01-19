using Fig.Client.Abstractions.Attributes;
using Fig.Client.Abstractions.Enums;
using Fig.Client.Abstractions.Validation;

namespace TicketSystem.Team.Application.Configuration;

public class TeamSettings
{
    private const string MarkdownKey = "$TicketSystemSettings";

    [Setting($"{MarkdownKey}#MinNameLength")]
    [Category("Validation", "#95E1D3")]
    [Validation(ValidationType.GreaterThanZero)]
    public int MinNameLength { get; set; } = 2;

    [Setting($"{MarkdownKey}#MaxNameLength")]
    [Category("Validation", "#95E1D3")]
    [ValidateIsBetween(5, 100, Inclusion.Inclusive)]
    public int MaxNameLength { get; set; } = 50;

    [Setting($"{MarkdownKey}#Team MaxDescriptionLength")]
    [Category("Validation", "#95E1D3")]
    [ValidateIsBetween(0, 2000, Inclusion.Inclusive)]
    public int MaxDescriptionLength { get; set; } = 1000;

    [Setting($"{MarkdownKey}#MaxTeamMembers")]
    [Category("Business Rules", "#38A3A5")]
    [ValidateIsBetween(1, 1000, Inclusion.Inclusive)]
    public int MaxTeamMembers { get; set; } = 100;

    [Setting($"{MarkdownKey}#MinTeamMembers")]
    [Category("Business Rules", "#38A3A5")]
    [ValidateIsBetween(0, 10, Inclusion.Inclusive)]
    public int MinTeamMembers { get; set; } = 1;

    [Setting($"{MarkdownKey}#AllowEmptyTeams")]
    [Category("Business Rules", "#38A3A5")]
    public bool AllowEmptyTeams { get; set; } = true;

    [Setting($"{MarkdownKey}#RequireTeamLead")]
    [Category("Business Rules", "#38A3A5")]
    public bool RequireTeamLead { get; set; } = false;
}
