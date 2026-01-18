using Fig.Client.Abstractions.Attributes;
using Fig.Client.Abstractions.Enums;
using Fig.Client.Abstractions.Validation;

namespace TicketSystem.Issue.Configuration;

public class IssueSettings
{
    [Setting("Minimum length for issue title")]
    [Category("Validation", "#FF6B6B")]
    [Validation(ValidationType.GreaterThanZero)]
    public int MinTitleLength { get; set; } = 3;

    [Setting("Maximum length for issue title")]
    [Category("Validation", "#FF6B6B")]
    [ValidateIsBetween(10, 500, Inclusion.Inclusive)]
    public int MaxTitleLength { get; set; } = 200;

    [Setting("Maximum length for issue description")]
    [Category("Validation", "#FF6B6B")]
    [ValidateIsBetween(0, 10000, Inclusion.Inclusive)]
    public int MaxDescriptionLength { get; set; } = 5000;

    [Fig.Client.Abstractions.Attributes.Setting("Number of days before due date to show warning")]
    [Category("Business Rules", "#4ECDC4")]
    [ValidateIsBetween(1, 30, Inclusion.Inclusive)]
    public int DueDateWarningDays { get; set; } = 3;

    [Setting("Allow issues to be created without due dates")]
    [Category("Business Rules", "#4ECDC4")]
    public bool AllowNoDueDate { get; set; } = true;

    [Setting("Default priority for new issues")]
    [Category("Business Rules", "#4ECDC4")]
    [ValidValues("Low", "Medium", "High", "Critical")]
    public string DefaultPriority { get; set; } = "Medium";

    [Setting("Automatically resolve issues when marked as complete")]
    [Category("Business Rules", "#4ECDC4")]
    public bool AutoResolveOnComplete { get; set; } = true;
}
