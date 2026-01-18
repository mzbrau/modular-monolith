using TicketSystem.Testing.Common.Builders;
using TicketSystem.User.Contracts;

namespace TicketSystem.User.TestBuilders;

/// <summary>
/// Fluent builder for creating test users via IUserModuleApi.
/// </summary>
public class UserBuilder : BuilderBase<UserBuilder, CreateUserRequest>
{
    private readonly IUserModuleApi _userModuleApi;
    
    private string _firstName = "Test";
    private string _lastName = "User";
    private string _email = $"test.user.{Guid.NewGuid():N}@example.com";
    private long? _createdUserId;

    public UserBuilder(IUserModuleApi userModuleApi)
    {
        _userModuleApi = userModuleApi;
    }
    
    public long CreatedUserId => _createdUserId ?? throw new InvalidOperationException("User has not been created yet. Call CreateAsync first.");

    public UserBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return This;
    }

    public UserBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return This;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return This;
    }

    public UserBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return This;
    }

    public override CreateUserRequest Build()
    {
        return new CreateUserRequest
        {
            FirstName = _firstName,
            LastName = _lastName,
            Email = _email
        };
    }

    /// <summary>
    /// Creates the user in the system and returns the request.
    /// </summary>
    public override async Task<CreateUserRequest> CreateAsync()
    {
        var request = Build();
        var response = await _userModuleApi.CreateUserAsync(request);
        _createdUserId = response.UserId;
        return request;
    }

    /// <summary>
    /// Creates the user and returns the full user data contract.
    /// </summary>
    public async Task<UserDataContract> CreateAndGetAsync()
    {
        await CreateAsync();
        var user = await _userModuleApi.GetUserAsync(new GetUserRequest { UserId = CreatedUserId });
        return user ?? throw new InvalidOperationException("User was created but could not be retrieved");
    }
}
