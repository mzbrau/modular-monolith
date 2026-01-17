using System.Reflection;
using NetArchTest.Rules;

namespace TicketSystem.Architecture.Tests;

[TestFixture]
public class UserModuleLayerTests
{
    private static readonly Assembly UserModuleAssembly = typeof(User.Domain.UserBusinessEntity).Assembly;

    [Test]
    public void Domain_ShouldNotDependOn_Application()
    {
        var result = Types.InAssembly(UserModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.User.Domain")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Application")
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
        var result = Types.InAssembly(UserModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.User.Domain")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Infrastructure")
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
        var result = Types.InAssembly(UserModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.User.Application")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Infrastructure")
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
