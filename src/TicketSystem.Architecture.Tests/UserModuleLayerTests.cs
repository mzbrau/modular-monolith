using System.Reflection;
using NetArchTest.Rules;

namespace TicketSystem.Architecture.Tests;

[TestFixture]
public class UserModuleLayerTests
{
    private static readonly Assembly UserModuleAssembly = typeof(User.Domain.UserId).Assembly;

    [Test]
    public void Domain_ShouldNotDependOn_Application()
    {
        var result = Types.InAssembly(UserModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.User.Domain")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.User.Application")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Domain layer should not depend on Application layer");
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

        Assert.That(result.IsSuccessful, Is.True,
            "Domain layer should not depend on Infrastructure layer");
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

        Assert.That(result.IsSuccessful, Is.True,
            "Application layer should not depend on Infrastructure layer");
    }
}
