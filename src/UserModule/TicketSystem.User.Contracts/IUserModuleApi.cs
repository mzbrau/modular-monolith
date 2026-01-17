namespace TicketSystem.User.Contracts;

public interface IUserModuleApi
{
    Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserDataContract?> GetUserAsync(GetUserRequest request);
    Task<List<UserDataContract>> GetAllUsersAsync();
    Task<List<UserDataContract>> GetUsersAsync(GetUsersByIdsRequest request);
    Task UpdateUserAsync(UpdateUserRequest request);
    Task DeactivateUserAsync(DeactivateUserRequest request);
    Task<UserDataContract?> FindUserByEmailAsync(FindUserByEmailRequest request);
    Task<bool> UserExistsAsync(UserExistsRequest request);
}
