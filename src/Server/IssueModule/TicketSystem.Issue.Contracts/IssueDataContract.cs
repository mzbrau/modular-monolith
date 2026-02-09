using System;

namespace TicketSystem.Issue.Contracts;

public class IssueDataContract
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public int Priority { get; set; }
    public long? AssignedUserId { get; set; }
    public long? AssignedTeamId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
}
