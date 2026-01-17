namespace TicketSystem.Issue.Contracts;

public class UpdateIssueRequest
{
    public long IssueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime? DueDate { get; set; }
}
