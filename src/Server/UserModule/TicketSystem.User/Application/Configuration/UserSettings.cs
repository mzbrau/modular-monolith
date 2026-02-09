using Fig.Client.Abstractions.Attributes;
using Fig.Client.Abstractions.Enums;
using Fig.Client.Abstractions.Validation;

namespace TicketSystem.User.Application.Configuration;

public class UserSettings
{
    private const string MarkdownKey = "$TicketSystemSettings";

    [Setting($"{MarkdownKey}#MinFirstNameLength")]
    [Category("Validation", "#FFA07A")]
    [Validation(ValidationType.GreaterThanZero)]
    public int MinFirstNameLength { get; set; } = 1;

    [Setting($"{MarkdownKey}#MaxFirstNameLength")]
    [Category("Validation", "#FFA07A")]
    [ValidateIsBetween(2, 50, Inclusion.Inclusive)]
    public int MaxFirstNameLength { get; set; } = 30;

    [Setting($"{MarkdownKey}#MinLastNameLength")]
    [Category("Validation", "#FFA07A")]
    [Validation(ValidationType.GreaterThanZero)]
    public int MinLastNameLength { get; set; } = 1;

    [Setting($"{MarkdownKey}#MaxLastNameLength")]
    [Category("Validation", "#FFA07A")]
    [ValidateIsBetween(2, 50, Inclusion.Inclusive)]
    public int MaxLastNameLength { get; set; } = 30;

    [Setting($"{MarkdownKey}#AllowedEmailDomains")]
    [Category("Security", "#FF6347")]
    [MultiLine(3)]
    public string? AllowedEmailDomains { get; set; } = "";

    [Setting($"{MarkdownKey}#RequireEmailVerification")]
    [Category("Security", "#FF6347")]
    public bool RequireEmailVerification { get; set; } = false;

    [Setting($"{MarkdownKey}#AutoDeactivateAfterDays")]
    [Category("Security", "#FF6347")]
    [ValidateIsBetween(0, 365, Inclusion.Inclusive)]
    public int AutoDeactivateAfterDays { get; set; } = 0;

    [Setting($"{MarkdownKey}#AllowPermanentDelete")]
    [Category("Security", "#FF6347")]
    public bool AllowPermanentDelete { get; set; } = false;
}
