using TicketSystem.Testing.Common.Fixtures;
using TicketSystem.User.Contracts;
using TicketSystem.User.TestBuilders;

namespace TicketSystem.User.IntegrationTests.ApiTests;

[TestFixture]
public class UserModuleApiTests : ModuleTestBase
{
    private IUserModuleApi _userApi = null!;
    private UserTestDataSeeder _users = null!;

    [OneTimeSetUp]
    public override async Task TestClassSetUp()
    {
        await base.TestClassSetUp();
        
        _userApi = GetService<IUserModuleApi>();
        _users = new UserTestDataSeeder(_userApi);
    }

    [Test]
    public async Task CreateUser_WithValidData_ReturnsUserId()
    {
        // Arrange
        var request = _users.AUser()
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmail("john.doe@example.com")
            .Build();

        // Act
        var response = await _userApi.CreateUserAsync(request);

        // Assert
        Assert.That(response.UserId, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetUser_AfterCreate_ReturnsCorrectData()
    {
        // Arrange
        var user = await _users.AUser()
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .WithEmail("jane.smith@example.com")
            .CreateAndGetAsync();

        // Act
        var retrieved = await _userApi.GetUserAsync(
            new GetUserRequest { UserId = user.Id });

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.FirstName, Is.EqualTo("Jane"));
        Assert.That(retrieved.LastName, Is.EqualTo("Smith"));
        Assert.That(retrieved.Email, Is.EqualTo("jane.smith@example.com"));
    }

    [Test]
    public async Task GetAllUsers_ReturnsAllCreatedUsers()
    {
        // Arrange
        var user1 = await _users.AUser()
            .WithName("Alice", "Anderson")
            .CreateAndGetAsync();
        
        var user2 = await _users.AUser()
            .WithName("Bob", "Brown")
            .CreateAndGetAsync();

        // Act
        var allUsers = await _userApi.GetAllUsersAsync();

        // Assert
        Assert.That(allUsers, Is.Not.Empty);
        Assert.That(allUsers.Any(u => u.Id == user1.Id), Is.True);
        Assert.That(allUsers.Any(u => u.Id == user2.Id), Is.True);
    }

    [Test]
    public async Task UpdateUser_WithValidData_UpdatesUserInformation()
    {
        // Arrange
        var user = await _users.AUser()
            .WithName("Original", "Name")
            .CreateAndGetAsync();

        // Act
        await _userApi.UpdateUserAsync(new UpdateUserRequest
        {
            UserId = user.Id,
            FirstName = "Updated",
            LastName = "Name"
        });

        // Assert
        var updated = await _userApi.GetUserAsync(new GetUserRequest { UserId = user.Id });
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.FirstName, Is.EqualTo("Updated"));
        Assert.That(updated.LastName, Is.EqualTo("Name"));
    }

    [Test]
    public async Task DeactivateUser_MarksUserAsInactive()
    {
        // Arrange
        var user = await _users.AUser()
            .WithName("Active", "User")
            .CreateAndGetAsync();

        // Act
        await _userApi.DeactivateUserAsync(new DeactivateUserRequest { UserId = user.Id });

        // Assert
        var deactivated = await _userApi.GetUserAsync(new GetUserRequest { UserId = user.Id });
        Assert.That(deactivated, Is.Not.Null);
        Assert.That(deactivated!.IsActive, Is.False);
    }

    [Test]
    public async Task FindUserByEmail_WithExistingEmail_ReturnsUser()
    {
        // Arrange
        var email = $"unique.{Guid.NewGuid():N}@example.com";
        var user = await _users.AUser()
            .WithEmail(email)
            .CreateAndGetAsync();

        // Act
        var found = await _userApi.FindUserByEmailAsync(new FindUserByEmailRequest { Email = email });

        // Assert
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(user.Id));
        Assert.That(found.Email, Is.EqualTo(email));
    }

    [Test]
    public async Task FindUserByEmail_WithNonExistingEmail_ReturnsNull()
    {
        // Act
        var found = await _userApi.FindUserByEmailAsync(
            new FindUserByEmailRequest { Email = "nonexistent@example.com" });

        // Assert
        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task UserExists_WithExistingUser_ReturnsTrue()
    {
        // Arrange
        var user = await _users.CreateStandardUserAsync();

        // Act
        var exists = await _userApi.UserExistsAsync(new UserExistsRequest { UserId = user.Id });

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task UserExists_WithNonExistingUser_ReturnsFalse()
    {
        // Act
        var exists = await _userApi.UserExistsAsync(new UserExistsRequest { UserId = 999999 });

        // Assert
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task GetUsersAsync_WithMultipleIds_ReturnsRequestedUsers()
    {
        // Arrange
        var user1 = await _users.AUser().CreateAndGetAsync();
        var user2 = await _users.AUser().CreateAndGetAsync();
        var user3 = await _users.AUser().CreateAndGetAsync();

        // Act
        var users = await _userApi.GetUsersAsync(new GetUsersByIdsRequest
        {
            UserIds = new List<long> { user1.Id, user3.Id }
        });

        // Assert
        Assert.That(users, Has.Count.EqualTo(2));
        Assert.That(users.Any(u => u.Id == user1.Id), Is.True);
        Assert.That(users.Any(u => u.Id == user3.Id), Is.True);
        Assert.That(users.Any(u => u.Id == user2.Id), Is.False);
    }
}
