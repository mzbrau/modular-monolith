using Fig.Client.Abstractions.Attributes;
using Fig.Client.Abstractions.Enums;
using Fig.Client.Abstractions.Validation;

namespace TicketSystem.User.Configuration;

public class UserSettings
{
    [Setting("Minimum length for first name")]
    [Category("Validation", "#FFA07A")]
    [Validation(ValidationType.GreaterThanZero)]
    public int MinFirstNameLength { get; set; } = 1;

    [Setting("Maximum length for first name")]
    [Category("Validation", "#FFA07A")]
    [ValidateIsBetween(2, 50, Inclusion.Inclusive)]
    public int MaxFirstNameLength { get; set; } = 30;

    [Setting("Minimum length for last name")]
    [Category("Validation", "#FFA07A")]
    [Validation(ValidationType.GreaterThanZero)]
    public int MinLastNameLength { get; set; } = 1;

    [Setting("Maximum length for last name")]
    [Category("Validation", "#FFA07A")]
    [ValidateIsBetween(2, 50, Inclusion.Inclusive)]
    public int MaxLastNameLength { get; set; } = 30;

    [Setting("Email domain whitelist (comma-separated, empty for all)")]
    [Category("Security", "#FF6347")]
    [MultiLine(3)]
    public string? AllowedEmailDomains { get; set; } = "";

    [Setting("Require email verification before activation")]
    [Category("Security", "#FF6347")]
    public bool RequireEmailVerification { get; set; } = false;

    [Setting("Automatically deactivate users after inactivity (days, 0 for never)")]
    [Category("Security", "#FF6347")]
    [ValidateIsBetween(0, 365, Inclusion.Inclusive)]
    public int AutoDeactivateAfterDays { get; set; } = 0;

    [Setting("Allow users to be permanently deleted")]
    [Category("Security", "#FF6347")]
    public bool AllowPermanentDelete { get; set; } = false;
}
