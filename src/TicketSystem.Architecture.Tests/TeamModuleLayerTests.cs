using System.Reflection;
using NetArchTest.Rules;

namespace TicketSystem.Architecture.Tests;

[TestFixture]
public class TeamModuleLayerTests
{
    private static readonly Assembly TeamModuleAssembly = typeof(Team.Domain.TeamBusinessEntity).Assembly;

    [Test]
    public void Domain_ShouldNotDependOn_Application()
    {
        var result = Types.InAssembly(TeamModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.Team.Domain")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Team.Application")
            .GetResult();

        var errorMessage = "Domain layer should not depend on Application layer";
        if (!result.IsSuccessful && result.FailingTypes != null)
        {
            var failingTypes = string.Join("\n  - ", result.FailingTypes.Select(t => t.FullName));
            errorMessage += $"\n\nFailing types:\n  - {failingTypes}";
        }

        Assert.That(result.IsSuccessful, Is.True, errorMessage);
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

        var errorMessage = "Domain layer should not depend on Infrastructure layer";
        if (!result.IsSuccessful && result.FailingTypes != null)
        {
            var failingTypes = string.Join("\n  - ", result.FailingTypes.Select(t => t.FullName));
            errorMessage += $"\n\nFailing types:\n  - {failingTypes}";
        }

        Assert.That(result.IsSuccessful, Is.True, errorMessage);
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

        var errorMessage = "Application layer should not depend on Infrastructure layer";
        if (!result.IsSuccessful && result.FailingTypes != null)
        {
            var failingTypes = string.Join("\n  - ", result.FailingTypes.Select(t => t.FullName));
            errorMessage += $"\n\nFailing types:\n  - {failingTypes}";
        }

        Assert.That(result.IsSuccessful, Is.True, errorMessage);
    }
}
