namespace TicketSystem.Issue.Contracts;

public class IssueDataContract
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public int Priority { get; set; }
    public Guid? AssignedUserId { get; set; }
    public Guid? AssignedTeamId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
}
