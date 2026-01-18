using TicketSystem.User.Contracts;

namespace TicketSystem.User.TestBuilders;

/// <summary>
/// Factory class for creating UserBuilders within a test context.
/// Accessed via the shared fixture.
/// </summary>
public class UserTestDataSeeder
{
    private readonly IUserModuleApi _userModuleApi;

    public UserTestDataSeeder(IUserModuleApi userModuleApi)
    {
        _userModuleApi = userModuleApi;
    }

    public UserBuilder AUser() => new UserBuilder(_userModuleApi);

    public UserBuilder AnAdmin() => new UserBuilder(_userModuleApi)
        .WithFirstName("Admin")
        .WithLastName("User");

    /// <summary>
    /// Creates a standard test user with default values.
    /// </summary>
    public async Task<UserDataContract> CreateStandardUserAsync()
    {
        return await AUser().CreateAndGetAsync();
    }
}
