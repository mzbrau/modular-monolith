using NetArchTest.Rules;
using System.Reflection;
using TicketSystem.Architecture.Tests.ExtensionMethods;

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
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
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
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
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
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }
}
