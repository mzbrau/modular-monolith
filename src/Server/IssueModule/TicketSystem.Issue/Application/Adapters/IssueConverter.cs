using TicketSystem.Issue.Contracts;
using TicketSystem.Issue.Domain;

namespace TicketSystem.Issue.Application.Adapters;

internal static class IssueConverter
{
    public static IssueDataContract ToDataContract(IssueBusinessEntity issue)
    {
        return new IssueDataContract
        {
            Id = issue.Id,
            Title = issue.Title,
            Description = issue.Description,
            Status = (int)issue.Status,
            Priority = (int)issue.Priority,
            AssignedUserId = issue.AssignedUserId,
            AssignedTeamId = issue.AssignedTeamId,
            DueDate = issue.DueDate,
            CreatedDate = issue.CreatedDate,
            LastModifiedDate = issue.LastModifiedDate,
            ResolvedDate = issue.ResolvedDate
        };
    }
}
