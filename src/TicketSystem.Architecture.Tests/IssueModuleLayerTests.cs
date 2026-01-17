using System.Reflection;
using NetArchTest.Rules;
using TicketSystem.Architecture.Tests.ExtensionMethods;

namespace TicketSystem.Architecture.Tests;

[TestFixture]
public class IssueModuleLayerTests
{
    private static readonly Assembly IssueModuleAssembly = typeof(Issue.Domain.IssueBusinessEntity).Assembly;

    [Test]
    public void Domain_ShouldNotDependOn_Application()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.Issue.Domain")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Issue.Application")
            .GetResult();

        var errorMessage = "Domain layer should not depend on Application layer";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }

    [Test]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.Issue.Domain")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Issue.Infrastructure")
            .GetResult();

        var errorMessage = "Domain layer should not depend on Infrastructure layer";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }

    [Test]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        TestResult? result = Types.InAssembly(IssueModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.Issue.Application")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Issue.Infrastructure")
            .GetResult();

        var errorMessage = "Application layer should not depend on Infrastructure layer";
        Assert.That(result.IsSuccessful, Is.True, result.GetDetails(errorMessage));
    }
}
