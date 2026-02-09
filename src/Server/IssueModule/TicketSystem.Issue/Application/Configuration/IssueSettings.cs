using Fig.Client.Abstractions.Attributes;
using Fig.Client.Abstractions.Enums;
using Fig.Client.Abstractions.Validation;

namespace TicketSystem.Issue.Application.Configuration;

public class IssueSettings
{
    private const string MarkdownKey = "$TicketSystemSettings";

    [Setting($"{MarkdownKey}#MinTitleLength")]
    [Category("Validation", "#FF6B6B")]
    [Validation(ValidationType.GreaterThanZero)]
    public int MinTitleLength { get; set; } = 3;

    [Setting($"{MarkdownKey}#MaxTitleLength")]
    [Category("Validation", "#FF6B6B")]
    [ValidateIsBetween(10, 500, Inclusion.Inclusive)]
    public int MaxTitleLength { get; set; } = 200;

    [Setting($"{MarkdownKey}#Issue MaxDescriptionLength")]
    [Category("Validation", "#FF6B6B")]
    [ValidateIsBetween(0, 10000, Inclusion.Inclusive)]
    public int MaxDescriptionLength { get; set; } = 5000;

    [Fig.Client.Abstractions.Attributes.Setting($"{MarkdownKey}#DueDateWarningDays")]
    [Category("Business Rules", "#4ECDC4")]
    [ValidateIsBetween(1, 30, Inclusion.Inclusive)]
    public int DueDateWarningDays { get; set; } = 3;

    [Setting($"{MarkdownKey}#AllowNoDueDate")]
    [Category("Business Rules", "#4ECDC4")]
    public bool AllowNoDueDate { get; set; } = true;

    [Setting($"{MarkdownKey}#DefaultPriority")]
    [Category("Business Rules", "#4ECDC4")]
    [ValidValues("Low", "Medium", "High", "Critical")]
    public string DefaultPriority { get; set; } = "Medium";

    [Setting($"{MarkdownKey}#AutoResolveOnComplete")]
    [Category("Business Rules", "#4ECDC4")]
    public bool AutoResolveOnComplete { get; set; } = true;
}
