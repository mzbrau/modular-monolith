using System;
using NUnit.Framework;
using TicketSystem.Issue.Domain;

namespace TicketSystem.Issue.Tests;

[TestFixture]
public class IssueBusinessEntityTests
{
    [Test]
    public void Constructor_WithValidData_CreatesIssue()
    {
        var issueId = 1L;
        const string title = "Bug in login";
        const string description = "Users cannot login";
        var priority = IssuePriority.High;
        var dueDate = DateTime.UtcNow.AddDays(7);

        var issue = new IssueBusinessEntity(issueId, title, description, priority, dueDate);

        Assert.That(issue.Id, Is.EqualTo(issueId));
        Assert.That(issue.Title, Is.EqualTo(title));
        Assert.That(issue.Description, Is.EqualTo(description));
        Assert.That(issue.Priority, Is.EqualTo(priority));
        Assert.That(issue.DueDate, Is.EqualTo(dueDate));
        Assert.That(issue.Status, Is.EqualTo(IssueStatus.Open));
        Assert.That(issue.AssignedUserId, Is.Null);
        Assert.That(issue.AssignedTeamId, Is.Null);
    }

    [Test]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new IssueBusinessEntity(1L, "", "Description", IssuePriority.Medium, null));
    }

    [Test]
    public void Update_WithValidData_UpdatesIssue()
    {
        var issue = new IssueBusinessEntity(1L, "Old Title", "Old Description", IssuePriority.Low, null);
        var newDueDate = DateTime.UtcNow.AddDays(14);

        issue.Update("New Title", "New Description", IssuePriority.Critical, newDueDate);

        Assert.That(issue.Title, Is.EqualTo("New Title"));
        Assert.That(issue.Description, Is.EqualTo("New Description"));
        Assert.That(issue.Priority, Is.EqualTo(IssuePriority.Critical));
        Assert.That(issue.DueDate, Is.EqualTo(newDueDate));
    }

    [Test]
    public void UpdateStatus_ToResolved_SetsResolvedDate()
    {
        var issue = new IssueBusinessEntity(1L, "Bug", "Description", IssuePriority.Medium, null);

        issue.UpdateStatus(IssueStatus.Resolved);

        Assert.That(issue.Status, Is.EqualTo(IssueStatus.Resolved));
        Assert.That(issue.ResolvedDate, Is.Not.Null);
    }

    [Test]
    public void UpdateStatus_FromResolvedToInProgress_ClearsResolvedDate()
    {
        var issue = new IssueBusinessEntity(1L, "Bug", "Description", IssuePriority.Medium, null);
        issue.UpdateStatus(IssueStatus.Resolved);

        issue.UpdateStatus(IssueStatus.InProgress);

        Assert.That(issue.Status, Is.EqualTo(IssueStatus.InProgress));
        Assert.That(issue.ResolvedDate, Is.Null);
    }

    [Test]
    public void AssignToUser_SetsAssignedUserId()
    {
        var issue = new IssueBusinessEntity(1L, "Bug", "Description", IssuePriority.Medium, null);
        long userId = 1;

        issue.AssignToUser(userId);

        Assert.That(issue.AssignedUserId, Is.EqualTo(userId));
    }

    [Test]
    public void AssignToTeam_SetsAssignedTeamId()
    {
        var issue = new IssueBusinessEntity(1L, "Bug", "Description", IssuePriority.Medium, null);
        long teamId = 1;

        issue.AssignToTeam(teamId);

        Assert.That(issue.AssignedTeamId, Is.EqualTo(teamId));
    }
}
