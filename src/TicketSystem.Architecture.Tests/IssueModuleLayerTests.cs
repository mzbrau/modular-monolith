using System.Reflection;
using NetArchTest.Rules;

namespace TicketSystem.Architecture.Tests;

[TestFixture]
public class IssueModuleLayerTests
{
    private static readonly Assembly IssueModuleAssembly = typeof(Issue.Domain.IssueId).Assembly;

    [Test]
    public void Domain_ShouldNotDependOn_Application()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.Issue.Domain")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Issue.Application")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Domain layer should not depend on Application layer");
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

        Assert.That(result.IsSuccessful, Is.True,
            "Domain layer should not depend on Infrastructure layer");
    }

    [Test]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(IssueModuleAssembly)
            .That()
            .ResideInNamespace("TicketSystem.Issue.Application")
            .ShouldNot()
            .HaveDependencyOn("TicketSystem.Issue.Infrastructure")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Application layer should not depend on Infrastructure layer");
    }
}
