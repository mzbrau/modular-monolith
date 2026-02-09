using NUnit.Framework;
using TicketSystem.Testing.Common.Fixtures;

namespace TicketSystem.Issue.IntegrationTests;

/// <summary>
/// Test fixture for Issue module integration tests.
/// Inherits the shared factory and adds module-specific setup.
/// </summary>
[SetUpFixture]
public class IssueModuleTestFixture : IntegrationTestFixture
{
    [OneTimeSetUp]
    public override async Task GlobalSetUp()
    {
        await base.GlobalSetUp();
        
        // Any Issue module-specific setup
    }
}
