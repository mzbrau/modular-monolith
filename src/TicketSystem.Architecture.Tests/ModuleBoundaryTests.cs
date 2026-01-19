using System.Reflection;
using NetArchTest.Rules;
using TicketSystem.Architecture.Tests.ExtensionMethods;

namespace TicketSystem.Architecture.Tests;

[TestFixture]
public class ModuleBoundaryTests
{
    private static readonly Assembly UserModuleAssembly = typeof(User.Domain.UserBusinessEntity).Assembly;
    private static readonly Assembly TeamModuleAssembly = typeof(Team.Domain.TeamBusinessEntity).Assembly;
    private static readonly Assembly IssueModuleAssembly = typeof(Issue.Domain.IssueBusinessEntity).Assembly;

    [Test]
    public void UserModule_ShouldNotReference_TeamModule()
    {
        var result = Types.InAssembly(UserModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Team")
            .GetResult();

        var errorMessage = "User module should not reference Team module";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }

    [Test]
    public void UserModule_ShouldNotReference_IssueModule()
    {
        var result = Types.InAssembly(UserModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Issue")
            .GetResult();

        var errorMessage = "User module should not reference Issue module";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }

    [Test]
    public void TeamModule_ShouldNotReference_IssueModule()
    {
        var result = Types.InAssembly(TeamModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Issue")
            .GetResult();

        var errorMessage = "Team module should not reference Issue module";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }

    [Test]
    public void TeamModule_ShouldNotReference_UserDomain()
    {
        var result = Types.InAssembly(TeamModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Domain")
            .GetResult();

        var errorMessage = "Team module should not reference User.Domain directly";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }

    [Test]
    public void TeamModule_ShouldNotReference_UserApplication()
    {
        var result = Types.InAssembly(TeamModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Application")
            .GetResult();

        var errorMessage = "Team module should not reference User.Application directly";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }

    [Test]
    public void IssueModule_ShouldNotReference_UserDomain()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Domain")
            .GetResult();

        var errorMessage = "Issue module should not reference User.Domain directly";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }

    [Test]
    public void IssueModule_ShouldNotReference_UserApplication()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Application")
            .GetResult();

        var errorMessage = "Issue module should not reference User.Application directly";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }

    [Test]
    public void IssueModule_ShouldNotReference_TeamDomain()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Team.Domain")
            .GetResult();

        var errorMessage = "Issue module should not reference Team.Domain directly";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }

    [Test]
    public void IssueModule_ShouldNotReference_TeamApplication()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Team.Application")
            .GetResult();

        var errorMessage = "Issue module should not reference Team.Application directly";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }
}
