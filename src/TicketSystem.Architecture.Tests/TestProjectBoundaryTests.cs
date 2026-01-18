using System.Reflection;
using NetArchTest.Rules;

namespace TicketSystem.Architecture.Tests;

[TestFixture]
public class TestProjectBoundaryTests
{
    // Test Builder assemblies
    private static readonly Assembly UserTestBuildersAssembly = 
        typeof(User.TestBuilders.UserBuilder).Assembly;
    private static readonly Assembly TeamTestBuildersAssembly = 
        typeof(Team.TestBuilders.TeamBuilder).Assembly;
    private static readonly Assembly IssueTestBuildersAssembly = 
        typeof(Issue.TestBuilders.IssueBuilder).Assembly;

    // Integration Test assemblies
    private static readonly Assembly UserIntegrationTestsAssembly = 
        typeof(User.IntegrationTests.UserModuleTestFixture).Assembly;
    private static readonly Assembly TeamIntegrationTestsAssembly = 
        typeof(Team.IntegrationTests.TeamModuleTestFixture).Assembly;
    private static readonly Assembly IssueIntegrationTestsAssembly = 
        typeof(Issue.IntegrationTests.IssueModuleTestFixture).Assembly;
    
    [Test]
    public void UserTestBuilders_ShouldNotReference_UserDomainOrApplication()
    {
        var result = Types.InAssembly(UserTestBuildersAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.User.Domain",
                "TicketSystem.User.Application",
                "TicketSystem.User.Infrastructure")
            .GetResult();

        AssertArchitectureResult(result, 
            "User.TestBuilders should only reference User.Contracts, not internal modules");
    }

    [Test]
    public void UserTestBuilders_ShouldNotReference_OtherModules()
    {
        var result = Types.InAssembly(UserTestBuildersAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.Team",
                "TicketSystem.Issue")
            .GetResult();

        AssertArchitectureResult(result, 
            "User.TestBuilders should not reference other modules");
    }

    [Test]
    public void TeamTestBuilders_ShouldNotReference_TeamDomainOrApplication()
    {
        var result = Types.InAssembly(TeamTestBuildersAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.Team.Domain",
                "TicketSystem.Team.Application",
                "TicketSystem.Team.Infrastructure")
            .GetResult();

        AssertArchitectureResult(result, 
            "Team.TestBuilders should only reference Team.Contracts, not internal modules");
    }

    [Test]
    public void TeamTestBuilders_ShouldNotReference_OtherModuleInternals()
    {
        var result = Types.InAssembly(TeamTestBuildersAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.User.Domain",
                "TicketSystem.User.Application",
                "TicketSystem.User.Infrastructure",
                "TicketSystem.Issue")
            .GetResult();

        AssertArchitectureResult(result, 
            "Team.TestBuilders should not reference other modules' internals");
    }

    [Test]
    public void IssueTestBuilders_ShouldNotReference_IssueDomainOrApplication()
    {
        var result = Types.InAssembly(IssueTestBuildersAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.Issue.Domain",
                "TicketSystem.Issue.Application",
                "TicketSystem.Issue.Infrastructure")
            .GetResult();

        AssertArchitectureResult(result, 
            "Issue.TestBuilders should only reference Issue.Contracts, not internal modules");
    }

    [Test]
    public void IssueTestBuilders_ShouldNotReference_OtherModuleInternals()
    {
        var result = Types.InAssembly(IssueTestBuildersAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.User.Domain",
                "TicketSystem.User.Application",
                "TicketSystem.User.Infrastructure",
                "TicketSystem.Team.Domain",
                "TicketSystem.Team.Application",
                "TicketSystem.Team.Infrastructure")
            .GetResult();

        AssertArchitectureResult(result, 
            "Issue.TestBuilders should not reference other modules' internals");
    }

    [Test]
    public void UserIntegrationTests_ShouldNotReference_UserInternals()
    {
        var result = Types.InAssembly(UserIntegrationTestsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.User.Domain",
                "TicketSystem.User.Application",
                "TicketSystem.User.Infrastructure")
            .GetResult();

        AssertArchitectureResult(result, 
            "User.IntegrationTests should use contracts and builders, not internal modules");
    }

    [Test]
    public void TeamIntegrationTests_ShouldNotReference_TeamInternals()
    {
        var result = Types.InAssembly(TeamIntegrationTestsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.Team.Domain",
                "TicketSystem.Team.Application",
                "TicketSystem.Team.Infrastructure")
            .GetResult();

        AssertArchitectureResult(result, 
            "Team.IntegrationTests should use contracts and builders, not internal modules");
    }

    [Test]
    public void IssueIntegrationTests_ShouldNotReference_IssueInternals()
    {
        var result = Types.InAssembly(IssueIntegrationTestsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.Issue.Domain",
                "TicketSystem.Issue.Application",
                "TicketSystem.Issue.Infrastructure")
            .GetResult();

        AssertArchitectureResult(result, 
            "Issue.IntegrationTests should use contracts and builders, not internal modules");
    }

    [Test]
    public void IssueIntegrationTests_ShouldNotReference_OtherModuleInternals()
    {
        // Issue tests can use User and Team TestBuilders, but not their internals
        var result = Types.InAssembly(IssueIntegrationTestsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.User.Domain",
                "TicketSystem.User.Application",
                "TicketSystem.User.Infrastructure",
                "TicketSystem.Team.Domain",
                "TicketSystem.Team.Application",
                "TicketSystem.Team.Infrastructure")
            .GetResult();

        AssertArchitectureResult(result, 
            "Issue.IntegrationTests should not reference User or Team module internals");
    }

    [Test]
    public void TeamIntegrationTests_ShouldNotReference_OtherModuleInternals()
    {
        // Team tests can use User TestBuilders, but not User internals
        var result = Types.InAssembly(TeamIntegrationTestsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.User.Domain",
                "TicketSystem.User.Application",
                "TicketSystem.User.Infrastructure",
                "TicketSystem.Issue")
            .GetResult();

        AssertArchitectureResult(result, 
            "Team.IntegrationTests should not reference other module internals");
    }

    [Test]
    public void AllTestBuilders_CanReference_TestingCommon()
    {
        // This is an ALLOWED dependency - verify builders reference the common testing infrastructure
        var userBuilderTypes = Types.InAssembly(UserTestBuildersAssembly)
            .That()
            .HaveDependencyOn("TicketSystem.Testing.Common")
            .GetTypes();

        var teamBuilderTypes = Types.InAssembly(TeamTestBuildersAssembly)
            .That()
            .HaveDependencyOn("TicketSystem.Testing.Common")
            .GetTypes();

        var issueBuilderTypes = Types.InAssembly(IssueTestBuildersAssembly)
            .That()
            .HaveDependencyOn("TicketSystem.Testing.Common")
            .GetTypes();

        // All test builders should reference Testing.Common
        Assert.That(userBuilderTypes.Any() || teamBuilderTypes.Any() || issueBuilderTypes.Any(), 
            Is.True, 
            "At least one TestBuilder should reference Testing.Common");
    }

    private static void AssertArchitectureResult(TestResult result, string errorMessage)
    {
        if (result is { IsSuccessful: false, FailingTypes: not null })
        {
            var failingTypes = string.Join("\n  - ", 
                result.FailingTypes.Select(t => t.FullName));
            errorMessage += $"\n\nFailing types:\n  - {failingTypes}";
        }

        Assert.That(result.IsSuccessful, Is.True, errorMessage);
    }
}
