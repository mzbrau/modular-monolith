using TicketSystem.Testing.Common.Fixtures;
using TicketSystem.Issue.Contracts;
using TicketSystem.TestBuilders;
using TicketSystem.Team.Contracts;

using TicketSystem.User.Contracts;


namespace TicketSystem.Issue.IntegrationTests.ApiTests;

/// <summary>
/// Tests for cross-module functionality where issues interact with users and teams.
/// Demonstrates the power of test builders for setting up complex scenarios.
/// </summary>
[TestFixture]
public class IssueAssignmentTests : ModuleTestBase
{
    private IIssueModuleApi _issueApi = null!;
    private IUserModuleApi _userApi = null!;
    private ITeamModuleApi _teamApi = null!;
    
    private IssueTestDataSeeder _issues = null!;
    private UserTestDataSeeder _users = null!;
    private TeamTestDataSeeder _teams = null!;

    [OneTimeSetUp]
    public override async Task TestClassSetUp()
    {
        await base.TestClassSetUp();
        
        // Get module APIs from DI
        _issueApi = GetService<IIssueModuleApi>();
        _userApi = GetService<IUserModuleApi>();
        _teamApi = GetService<ITeamModuleApi>();
        
        // Initialize seeders
        _issues = new IssueTestDataSeeder(_issueApi);
        _users = new UserTestDataSeeder(_userApi);
        _teams = new TeamTestDataSeeder(_teamApi);
    }

    [Test]
    public async Task AssignIssueToUser_WithValidUser_Succeeds()
    {
        // Arrange - Create prerequisite data via module APIs
        var user = await _users.AUser()
            .WithFirstName("Developer")
            .WithLastName("One")
            .CreateAndGetAsync();

        var issue = await _issues.AnIssue()
            .WithTitle("Bug to assign")
            .CreateAndGetAsync();

        // Act
        await _issueApi.AssignIssueToUserAsync(new AssignIssueToUserRequest
        {
            IssueId = issue.Id,
            UserId = user.Id
        });

        // Assert
        var updatedIssue = await _issueApi.GetIssueAsync(
            new GetIssueRequest { IssueId = issue.Id });
        
        Assert.That(updatedIssue, Is.Not.Null);
        Assert.That(updatedIssue!.AssignedUserId, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task AssignIssueToTeam_WithValidTeam_Succeeds()
    {
        // Arrange
        var team = await _teams.ATeam()
            .WithName("Backend Team")
            .CreateAndGetAsync();

        var issue = await _issues.AnIssue()
            .WithTitle("Feature request")
            .CreateAndGetAsync();

        // Act
        await _issueApi.AssignIssueToTeamAsync(new AssignIssueToTeamRequest
        {
            IssueId = issue.Id,
            TeamId = team.Id
        });

        // Assert
        var updatedIssue = await _issueApi.GetIssueAsync(
            new GetIssueRequest { IssueId = issue.Id });
        
        Assert.That(updatedIssue, Is.Not.Null);
        Assert.That(updatedIssue!.AssignedTeamId, Is.EqualTo(team.Id));
    }

    [Test]
    public async Task GetIssuesByUser_ReturnsOnlyAssignedIssues()
    {
        // Arrange
        var user1 = await _users.AUser().CreateAndGetAsync();
        var user2 = await _users.AUser().CreateAndGetAsync();

        var issue1 = await _issues.AnIssue()
            .AssignedToUser(user1.Id)
            .CreateAndGetAsync();
        
        var issue2 = await _issues.AnIssue()
            .AssignedToUser(user2.Id)
            .CreateAndGetAsync();

        var issue3 = await _issues.AnIssue()
            .AssignedToUser(user1.Id)
            .CreateAndGetAsync();

        // Act
        var user1Issues = await _issueApi.GetIssuesByUserAsync(
            new GetIssuesByUserRequest { UserId = user1.Id });

        // Assert
        Assert.That(user1Issues, Has.Count.EqualTo(2));
        Assert.That(user1Issues.Any(i => i.Id == issue1.Id), Is.True);
        Assert.That(user1Issues.Any(i => i.Id == issue3.Id), Is.True);
        Assert.That(user1Issues.Any(i => i.Id == issue2.Id), Is.False);
    }

    [Test]
    public async Task GetIssuesByTeam_ReturnsOnlyTeamIssues()
    {
        // Arrange
        var team1 = await _teams.ATeam().WithName("Team 1").CreateAndGetAsync();
        var team2 = await _teams.ATeam().WithName("Team 2").CreateAndGetAsync();

        var issue1 = await _issues.AnIssue()
            .AssignedToTeam(team1.Id)
            .CreateAndGetAsync();
        
        var issue2 = await _issues.AnIssue()
            .AssignedToTeam(team2.Id)
            .CreateAndGetAsync();

        // Act
        var team1Issues = await _issueApi.GetIssuesByTeamAsync(
            new GetIssuesByTeamRequest { TeamId = team1.Id });

        // Assert
        Assert.That(team1Issues, Has.Count.EqualTo(1));
        Assert.That(team1Issues[0].Id, Is.EqualTo(issue1.Id));
    }

    [Test]
    public async Task ComplexScenario_TeamWithMembersAndIssues()
    {
        // Arrange - Complex setup using builders
        var teamLead = await _users.AUser()
            .WithName("Team", "Lead")
            .CreateAndGetAsync();
        
        var developer1 = await _users.AUser()
            .WithName("Junior", "Dev")
            .CreateAndGetAsync();
        
        var developer2 = await _users.AUser()
            .WithName("Senior", "Dev")
            .CreateAndGetAsync();

        var team = await _teams.ATeam()
            .WithName("Development Team")
            .WithMembers(teamLead.Id, developer1.Id, developer2.Id)
            .CreateAndGetAsync();

        var criticalBug = await _issues.AnIssue()
            .WithTitle("Critical production bug")
            .WithHighPriority()
            .AssignedToTeam(team.Id)
            .AssignedToUser(teamLead.Id)
            .CreateAndGetAsync();
        
        var featureRequest = await _issues.AnIssue()
            .WithTitle("New feature request")
            .WithLowPriority()
            .AssignedToTeam(team.Id)
            .AssignedToUser(developer2.Id)
            .CreateAndGetAsync();

        // Act
        var teamIssues = await _issueApi.GetIssuesByTeamAsync(
            new GetIssuesByTeamRequest { TeamId = team.Id });
        
        var teamLeadIssues = await _issueApi.GetIssuesByUserAsync(
            new GetIssuesByUserRequest { UserId = teamLead.Id });
        
        var teamMembers = await _teamApi.GetTeamMemberIdsAsync(
            new GetTeamMemberIdsRequest { TeamId = team.Id });

        // Assert
        Assert.That(teamIssues, Has.Count.EqualTo(2));
        Assert.That(teamLeadIssues, Has.Count.EqualTo(1));
        Assert.That(teamLeadIssues[0].Id, Is.EqualTo(criticalBug.Id));
        
        Assert.That(teamMembers, Has.Count.EqualTo(3));
        Assert.That(teamMembers, Does.Contain(teamLead.Id));
        Assert.That(teamMembers, Does.Contain(developer1.Id));
        Assert.That(teamMembers, Does.Contain(developer2.Id));
    }

    [Test]
    public async Task AssignIssue_BothUserAndTeam_BothAssignmentsStored()
    {
        // Arrange
        var user = await _users.CreateStandardUserAsync();
        var team = await _teams.CreateStandardTeamAsync();
        
        // Act
        var issue = await _issues.AnIssue()
            .AssignedToUser(user.Id)
            .AssignedToTeam(team.Id)
            .CreateAndGetAsync();

        // Assert
        Assert.That(issue.AssignedUserId, Is.EqualTo(user.Id));
        Assert.That(issue.AssignedTeamId, Is.EqualTo(team.Id));
    }

    [Test]
    public async Task CreateMultipleIssues_ForSameUser_AllRetrievedByUserQuery()
    {
        // Arrange
        var developer = await _users.AUser()
            .WithName("Busy", "Developer")
            .CreateAndGetAsync();

        // Act - Create multiple issues for the same user
        var bug = await _issues.ABug().AssignedToUser(developer.Id).CreateAndGetAsync();
        var criticalBug = await _issues.ACriticalBug().AssignedToUser(developer.Id).CreateAndGetAsync();
        var feature = await _issues.AnIssue()
            .WithTitle("Feature")
            .AssignedToUser(developer.Id)
            .CreateAndGetAsync();

        var userIssues = await _issueApi.GetIssuesByUserAsync(
            new GetIssuesByUserRequest { UserId = developer.Id });

        // Assert
        Assert.That(userIssues, Has.Count.EqualTo(3));
        var issueIds = userIssues.Select(i => i.Id).ToList();
        Assert.That(issueIds, Does.Contain(bug.Id));
        Assert.That(issueIds, Does.Contain(criticalBug.Id));
        Assert.That(issueIds, Does.Contain(feature.Id));
    }
}
