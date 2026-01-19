using TicketSystem.Testing.Common.Fixtures;
using TicketSystem.Issue.Contracts;
using TicketSystem.TestBuilders;

namespace TicketSystem.Issue.IntegrationTests.ApiTests;

[TestFixture]
public class IssueModuleApiTests : ModuleTestBase
{
    private IIssueModuleApi _issueApi = null!;
    private IssueTestDataSeeder _issues = null!;

    [OneTimeSetUp]
    public override async Task TestClassSetUp()
    {
        await base.TestClassSetUp();
        
        _issueApi = GetService<IIssueModuleApi>();
        _issues = new IssueTestDataSeeder(_issueApi);
    }

    [Test]
    public async Task CreateIssue_WithValidData_ReturnsIssueId()
    {
        // Arrange
        var request = _issues.AnIssue()
            .WithTitle("Bug in login system")
            .WithDescription("Users cannot login with valid credentials")
            .Build();

        // Act
        var response = await _issueApi.CreateIssueAsync(request);

        // Assert
        Assert.That(response.IssueId, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetIssue_AfterCreate_ReturnsCorrectData()
    {
        // Arrange
        var issue = await _issues.AnIssue()
            .WithTitle("Feature Request")
            .WithDescription("Add dark mode support")
            .CreateAndGetAsync();

        // Act
        var retrieved = await _issueApi.GetIssueAsync(
            new GetIssueRequest { IssueId = issue.Id });

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Title, Is.EqualTo("Feature Request"));
        Assert.That(retrieved.Description, Is.EqualTo("Add dark mode support"));
    }

    [Test]
    public async Task GetAllIssues_ReturnsAllCreatedIssues()
    {
        // Arrange
        var issue1 = await _issues.ABug().CreateAndGetAsync();
        var issue2 = await _issues.ACriticalBug().CreateAndGetAsync();

        // Act
        var allIssues = await _issueApi.GetAllIssuesAsync();

        // Assert
        Assert.That(allIssues, Is.Not.Empty);
        Assert.That(allIssues.Any(i => i.Id == issue1.Id), Is.True);
        Assert.That(allIssues.Any(i => i.Id == issue2.Id), Is.True);
    }

    [Test]
    public async Task UpdateIssue_WithValidData_UpdatesIssueInformation()
    {
        // Arrange
        var issue = await _issues.AnIssue()
            .WithTitle("Original Title")
            .WithDescription("Original description")
            .CreateAndGetAsync();

        // Act
        await _issueApi.UpdateIssueAsync(new UpdateIssueRequest
        {
            IssueId = issue.Id,
            Title = "Updated Title",
            Description = "Updated description",
            Priority = 1, // High
            DueDate = DateTime.UtcNow.AddDays(7)
        });

        // Assert
        var updated = await _issueApi.GetIssueAsync(new GetIssueRequest { IssueId = issue.Id });
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Title, Is.EqualTo("Updated Title"));
        Assert.That(updated.Description, Is.EqualTo("Updated description"));
        Assert.That(updated.Priority, Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateIssueStatus_ChangesIssueStatus()
    {
        // Arrange
        var issue = await _issues.CreateStandardIssueAsync();

        // Act - Change status to InProgress (1)
        await _issueApi.UpdateIssueStatusAsync(new UpdateIssueStatusRequest
        {
            IssueId = issue.Id,
            Status = 1 // InProgress
        });

        // Assert
        var updated = await _issueApi.GetIssueAsync(new GetIssueRequest { IssueId = issue.Id });
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Status, Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteIssue_RemovesIssueFromSystem()
    {
        // Arrange
        var issue = await _issues.CreateStandardIssueAsync();

        // Act
        await _issueApi.DeleteIssueAsync(new DeleteIssueRequest { IssueId = issue.Id });

        // Assert
        var deleted = await _issueApi.GetIssueAsync(new GetIssueRequest { IssueId = issue.Id });
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task CreateIssue_WithHighPriority_SetsCorrectPriority()
    {
        // Arrange & Act
        var issue = await _issues.ACriticalBug()
            .WithHighPriority()
            .CreateAndGetAsync();

        // Assert
        Assert.That(issue.Priority, Is.EqualTo(1)); // High priority
    }

    [Test]
    public async Task CreateIssue_WithDueDate_StoresDueDate()
    {
        // Arrange
        var dueDate = DateTime.UtcNow.AddDays(14);

        // Act
        var issue = await _issues.AnIssue()
            .WithDueDate(dueDate)
            .CreateAndGetAsync();

        // Assert
        Assert.That(issue.DueDate, Is.Not.Null);
        Assert.That(issue.DueDate!.Value.Date, Is.EqualTo(dueDate.Date));
    }
}
