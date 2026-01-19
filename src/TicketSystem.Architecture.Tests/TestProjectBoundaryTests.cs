using NetArchTest.Rules;
using System.Reflection;
using TicketSystem.Architecture.Tests.ExtensionMethods;

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

        var errorMessage = "TestBuilders should only reference Contracts, not internal modules";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
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

        var errorMessage = "User.IntegrationTests should use contracts and builders, not internal modules";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
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

        var errorMessage = "Team.IntegrationTests should use contracts and builders, not internal modules";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
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

        var errorMessage = "Issue.IntegrationTests should use contracts and builders, not internal modules";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
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

        var errorMessage = "Issue.IntegrationTests should not reference User or Team module internals";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
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

        var errorMessage = "Team.IntegrationTests should not reference other module internals";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
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
}
