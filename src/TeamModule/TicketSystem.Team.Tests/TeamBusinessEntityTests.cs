using TicketSystem.Team.Domain;

namespace TicketSystem.TeamModule.Tests;

[TestFixture]
public class TeamBusinessEntityTests
{
    [Test]
    public void Constructor_WithValidData_CreatesTeam()
    {
        var teamId = TeamId.New();
        const string name = "Dev Team";
        const string description = "Development team";

        var team = new TeamBusinessEntity(teamId, name, description);

        Assert.That(team.Id, Is.EqualTo(teamId));
        Assert.That(team.Name, Is.EqualTo(name));
        Assert.That(team.Description, Is.EqualTo(description));
        Assert.That(team.Members, Is.Empty);
    }

    [Test]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new TeamBusinessEntity(TeamId.New(), "", "Description"));
    }

    [Test]
    public void AddMember_WithNewUser_AddsMemberToTeam()
    {
        var team = new TeamBusinessEntity(TeamId.New(), "Dev Team", "Description");
        var userId = Guid.NewGuid();

        var member = team.AddMember(userId, TeamRole.Lead);

        Assert.That(team.Members.Count, Is.EqualTo(1));
        Assert.That(member.UserId, Is.EqualTo(userId));
        Assert.That(member.Role, Is.EqualTo(TeamRole.Lead));
    }

    [Test]
    public void AddMember_WithExistingUser_ThrowsInvalidOperationException()
    {
        var team = new TeamBusinessEntity(TeamId.New(), "Dev Team", "Description");
        var userId = Guid.NewGuid();
        team.AddMember(userId);

        Assert.Throws<InvalidOperationException>(() => team.AddMember(userId));
    }

    [Test]
    public void RemoveMember_WithExistingMember_RemovesMemberFromTeam()
    {
        var team = new TeamBusinessEntity(TeamId.New(), "Dev Team", "Description");
        var userId = Guid.NewGuid();
        team.AddMember(userId);

        team.RemoveMember(userId);

        Assert.That(team.Members, Is.Empty);
    }

    [Test]
    public void RemoveMember_WithNonExistingMember_ThrowsInvalidOperationException()
    {
        var team = new TeamBusinessEntity(TeamId.New(), "Dev Team", "Description");
        var userId = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(() => team.RemoveMember(userId));
    }

    [Test]
    public void HasMember_WithExistingMember_ReturnsTrue()
    {
        var team = new TeamBusinessEntity(TeamId.New(), "Dev Team", "Description");
        var userId = Guid.NewGuid();
        team.AddMember(userId);

        var result = team.HasMember(userId);

        Assert.That(result, Is.True);
    }
}
