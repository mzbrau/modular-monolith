using NUnit.Framework;
using TicketSystem.Testing.Common.Fixtures;

namespace TicketSystem.User.IntegrationTests;

/// <summary>
/// Test fixture for User module integration tests.
/// Inherits the shared factory and adds module-specific setup.
/// </summary>
[SetUpFixture]
public class UserModuleTestFixture : IntegrationTestFixture
{
    [OneTimeSetUp]
    public override async Task GlobalSetUp()
    {
        await base.GlobalSetUp();
        
        // Any User module-specific setup
    }
}
