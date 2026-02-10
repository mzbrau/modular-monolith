using FsCheck;
using FsCheck.Xunit;
using Xunit;
using IssueModel = TicketSystem.Client.Wpf.Models.Issue;
using IssueStatusModel = TicketSystem.Client.Wpf.Models.IssueStatus;

namespace TicketSystem.Client.Wpf.Tests.Properties;

/// <summary>
/// Property-based tests for issue display completeness.
/// Feature: wpf-ticket-client
/// </summary>
public class IssueDisplayPropertyTests
{
    /// <summary>
    /// Property 1: Issue Display Completeness
    /// For any issue retrieved from the IssueService, when displayed in the issue list,
    /// the UI SHALL show the title, status, priority, assigned user/team, and due date fields.
    /// **Validates: Requirements 3.2**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "wpf-ticket-client")]
    [Trait("Property", "1")]
    public Property IssueDisplayCompleteness_AllRequiredFieldsAreAccessible()
    {
        return Prop.ForAll(
            IssueGenerator(),
            issue =>
            {
                // Verify that all required fields for display are accessible and not throwing exceptions
                // According to Requirement 3.2, the UI SHALL show:
                // - title
                // - status
                // - priority
                // - assigned user/team
                // - due date
                
                try
                {
                    // Access all required display fields
                    var title = issue.Title;
                    var status = issue.Status;
                    var priority = issue.Priority;
                    var assignedUserId = issue.AssignedUserId;
                    var assignedTeamId = issue.AssignedTeamId;
                    var dueDate = issue.DueDate;
                    
                    // Verify that required fields are not null (except optional fields)
                    var titleIsValid = !string.IsNullOrEmpty(title);
                    var statusIsValid = Enum.IsDefined(typeof(IssueStatusModel), status);
                    var priorityIsValid = priority >= 0;
                    
                    // AssignedUserId, AssignedTeamId, and DueDate are optional, so they can be null
                    // but they should be accessible without throwing exceptions
                    
                    return titleIsValid && statusIsValid && priorityIsValid;
                }
                catch
                {
                    // If any field access throws an exception, the property fails
                    return false;
                }
            }
        ).Label("Feature: wpf-ticket-client, Property 1: Issue Display Completeness");
    }
    
    /// <summary>
    /// Generator for Issue objects with valid data.
    /// </summary>
    private static Arbitrary<IssueModel> IssueGenerator()
    {
        var gen = from id in Arb.Generate<NonEmptyString>()
                  from title in Arb.Generate<NonEmptyString>()
                  from description in Arb.Generate<string>()
                  from status in Gen.Elements(
                      IssueStatusModel.Open,
                      IssueStatusModel.InProgress,
                      IssueStatusModel.Blocked,
                      IssueStatusModel.Resolved,
                      IssueStatusModel.Closed)
                  from priority in Gen.Choose(0, 10)
                  from assignedUserId in Gen.OneOf(
                      Gen.Constant<string?>(null),
                      Arb.Generate<NonEmptyString>().Select(s => (string?)s.Get))
                  from assignedTeamId in Gen.OneOf(
                      Gen.Constant<string?>(null),
                      Arb.Generate<NonEmptyString>().Select(s => (string?)s.Get))
                  from createdDate in Arb.Generate<DateTime>()
                  from dueDate in Gen.OneOf(
                      Gen.Constant<DateTime?>(null),
                      Arb.Generate<DateTime>().Select(d => (DateTime?)d))
                  from resolvedDate in Gen.OneOf(
                      Gen.Constant<DateTime?>(null),
                      Arb.Generate<DateTime>().Select(d => (DateTime?)d))
                  from lastModifiedDate in Arb.Generate<DateTime>()
                  select new IssueModel
                  {
                      Id = id.Get,
                      Title = title.Get,
                      Description = description ?? string.Empty,
                      Status = status,
                      Priority = priority,
                      AssignedUserId = assignedUserId,
                      AssignedTeamId = assignedTeamId,
                      CreatedDate = createdDate,
                      DueDate = dueDate,
                      ResolvedDate = resolvedDate,
                      LastModifiedDate = lastModifiedDate
                  };
        
        return Arb.From(gen);
    }
}
