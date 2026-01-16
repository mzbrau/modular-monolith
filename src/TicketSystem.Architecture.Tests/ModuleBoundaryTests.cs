using System.Reflection;
using NetArchTest.Rules;

namespace TicketSystem.Architecture.Tests;

[TestFixture]
public class ModuleBoundaryTests
{
    private static readonly Assembly UserModuleAssembly = typeof(User.Domain.UserId).Assembly;
    private static readonly Assembly TeamModuleAssembly = typeof(Team.Domain.TeamId).Assembly;
    private static readonly Assembly IssueModuleAssembly = typeof(Issue.Domain.IssueId).Assembly;

    [Test]
    public void UserModule_ShouldNotReference_TeamModule()
    {
        var result = Types.InAssembly(UserModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Team")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "User module should not reference Team module");
    }

    [Test]
    public void UserModule_ShouldNotReference_IssueModule()
    {
        var result = Types.InAssembly(UserModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Issue")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "User module should not reference Issue module");
    }

    [Test]
    public void TeamModule_ShouldNotReference_IssueModule()
    {
        var result = Types.InAssembly(TeamModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Issue")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Team module should not reference Issue module");
    }

    [Test]
    public void TeamModule_ShouldNotReference_UserDomain()
    {
        var result = Types.InAssembly(TeamModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Domain")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Team module should not reference User.Domain directly");
    }

    [Test]
    public void TeamModule_ShouldNotReference_UserApplication()
    {
        var result = Types.InAssembly(TeamModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Application")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Team module should not reference User.Application directly");
    }

    [Test]
    public void IssueModule_ShouldNotReference_UserDomain()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Domain")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Issue module should not reference User.Domain directly");
    }

    [Test]
    public void IssueModule_ShouldNotReference_UserApplication()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Application")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Issue module should not reference User.Application directly");
    }

    [Test]
    public void IssueModule_ShouldNotReference_TeamDomain()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Team.Domain")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Issue module should not reference Team.Domain directly");
    }

    [Test]
    public void IssueModule_ShouldNotReference_TeamApplication()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Team.Application")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Issue module should not reference Team.Application directly");
    }
}
