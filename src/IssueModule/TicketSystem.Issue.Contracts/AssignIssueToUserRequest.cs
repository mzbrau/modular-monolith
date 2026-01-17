namespace TicketSystem.Issue.Contracts;

public class AssignIssueToUserRequest
{
    public long IssueId { get; set; }
    public long UserId { get; set; }
}
