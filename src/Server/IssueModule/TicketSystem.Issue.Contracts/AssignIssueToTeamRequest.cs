namespace TicketSystem.Issue.Contracts;

public class AssignIssueToTeamRequest
{
    public long IssueId { get; set; }
    public long? TeamId { get; set; }
}
