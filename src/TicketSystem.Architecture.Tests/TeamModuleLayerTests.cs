using System.Reflection;
using NetArchTest.Rules;

namespace TicketSystem.Architecture.Tests;

[TestFixture]
public class TeamModuleLayerTests
{
    private static readonly Assembly TeamModuleAssembly = typeof(Team.Domain.TeamId).Assembly;

    [Test]
    public void Domain_ShouldNotDependOn_Application()
    {
        var result = Types.InAssembly(TeamModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.Team.Domain")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Team.Application")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Domain layer should not depend on Application layer");
    }

    [Test]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(TeamModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.Team.Domain")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Team.Infrastructure")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Domain layer should not depend on Infrastructure layer");
    }

    [Test]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(TeamModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.Team.Application")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Team.Infrastructure")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Application layer should not depend on Infrastructure layer");
    }
}
