using System;

namespace TicketSystem.Issue.Contracts;

public class CreateIssueRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime? DueDate { get; set; }
}
