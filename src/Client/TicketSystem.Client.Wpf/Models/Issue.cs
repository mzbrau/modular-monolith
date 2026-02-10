namespace TicketSystem.Client.Wpf.Models;

/// <summary>
/// Represents an issue (work item or ticket) in the ticket system.
/// </summary>
public class Issue
{
    /// <summary>
    /// Gets or sets the unique identifier for the issue.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the issue.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the issue.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the issue.
    /// </summary>
    public IssueStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the priority level of the issue (higher number = higher priority).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user assigned to this issue, if any.
    /// </summary>
    public string? AssignedUserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the team assigned to this issue, if any.
    /// </summary>
    public string? AssignedTeamId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the issue was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the optional due date for the issue.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the issue was resolved, if applicable.
    /// </summary>
    public DateTime? ResolvedDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the issue was last modified.
    /// </summary>
    public DateTime LastModifiedDate { get; set; }
}
