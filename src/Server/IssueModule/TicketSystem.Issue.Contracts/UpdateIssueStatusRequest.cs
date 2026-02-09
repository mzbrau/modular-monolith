namespace TicketSystem.Issue.Contracts;

public class UpdateIssueStatusRequest
{
    public long IssueId { get; set; }
    public int Status { get; set; }
}
