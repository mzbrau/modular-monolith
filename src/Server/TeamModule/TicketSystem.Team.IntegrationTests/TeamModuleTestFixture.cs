using NUnit.Framework;
using TicketSystem.Testing.Common.Fixtures;

namespace TicketSystem.Team.IntegrationTests;

/// <summary>
/// Test fixture for Team module integration tests.
/// Inherits the shared factory and adds module-specific setup.
/// </summary>
[SetUpFixture]
public class TeamModuleTestFixture : IntegrationTestFixture
{
    [OneTimeSetUp]
    public override async Task GlobalSetUp()
    {
        await base.GlobalSetUp();
        
        // Any Team module-specific setup
    }
}
