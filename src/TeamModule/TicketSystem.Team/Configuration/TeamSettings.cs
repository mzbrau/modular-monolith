using Fig.Client.Abstractions.Attributes;
using Fig.Client.Abstractions.Enums;
using Fig.Client.Abstractions.Validation;

namespace TicketSystem.Team.Configuration;

public class TeamSettings
{
    [Setting("Minimum length for team name")]
    [Category("Validation", "#95E1D3")]
    [Validation(ValidationType.GreaterThanZero)]
    public int MinNameLength { get; set; } = 2;

    [Setting("Maximum length for team name")]
    [Category("Validation", "#95E1D3")]
    [ValidateIsBetween(5, 100, Inclusion.Inclusive)]
    public int MaxNameLength { get; set; } = 50;

    [Setting("Maximum length for team description")]
    [Category("Validation", "#95E1D3")]
    [ValidateIsBetween(0, 2000, Inclusion.Inclusive)]
    public int MaxDescriptionLength { get; set; } = 1000;

    [Setting("Maximum number of members allowed in a team")]
    [Category("Business Rules", "#38A3A5")]
    [ValidateIsBetween(1, 1000, Inclusion.Inclusive)]
    public int MaxTeamMembers { get; set; } = 100;

    [Setting("Minimum number of members required for a team")]
    [Category("Business Rules", "#38A3A5")]
    [ValidateIsBetween(0, 10, Inclusion.Inclusive)]
    public int MinTeamMembers { get; set; } = 1;

    [Setting("Allow teams to be created without members")]
    [Category("Business Rules", "#38A3A5")]
    public bool AllowEmptyTeams { get; set; } = true;

    [Setting("Require at least one team lead")]
    [Category("Business Rules", "#38A3A5")]
    public bool RequireTeamLead { get; set; } = false;
}
