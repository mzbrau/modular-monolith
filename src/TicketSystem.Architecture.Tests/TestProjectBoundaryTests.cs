using System.Reflection;
using NetArchTest.Rules;

namespace TicketSystem.Architecture.Tests;

[TestFixture]
public class TestProjectBoundaryTests
{
    // Test Builder assembly
    private static readonly Assembly TestBuildersAssembly = 
        typeof(TicketSystem.TestBuilders.UserBuilder).Assembly;

    // Integration Test assemblies
    private static readonly Assembly UserIntegrationTestsAssembly = 
        typeof(TicketSystem.User.IntegrationTests.UserModuleTestFixture).Assembly;
    private static readonly Assembly TeamIntegrationTestsAssembly = 
        typeof(TicketSystem.Team.IntegrationTests.TeamModuleTestFixture).Assembly;
    private static readonly Assembly IssueIntegrationTestsAssembly = 
        typeof(TicketSystem.Issue.IntegrationTests.IssueModuleTestFixture).Assembly;
    
    [Test]
    public void TestBuilders_ShouldNotReference_AnyModuleInternals()
    {
        var result = Types.InAssembly(TestBuildersAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TicketSystem.User.Domain",
                "TicketSystem.User.Application",
                "TicketSystem.User.Infrastructure",
                "TicketSystem.Team.Domain",
                "TicketSystem.Team.Application",
                "TicketSystem.Team.Infrastructure",
                "TicketSystem.Issue.Domain",
                "TicketSystem.Issue.Application",
                "TicketSystem.Issue.Infrastructure")
            .GetResult();

        AssertArchitectureResult(result, 
            "TestBuilders should only reference Contracts, not internal modules");
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
    public void TestBuilders_ShouldReference_TestingCommon()
    {
        // This is an ALLOWED dependency - verify builders reference the common testing infrastructure
        var builderTypes = Types.InAssembly(TestBuildersAssembly)
            .That()
            .HaveDependencyOn("TicketSystem.Testing.Common")
            .GetTypes();

        // All test builders should reference Testing.Common
        Assert.That(builderTypes.Any(), 
            Is.True, 
            "TestBuilders should reference Testing.Common");
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
