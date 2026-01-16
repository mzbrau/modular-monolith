namespace TicketSystem.User.Contracts;

public interface IUserModuleApi
{
    Task<Guid> CreateUserAsync(string email, string firstName, string lastName);
    Task<UserDataContract?> GetUserAsync(Guid userId);
    Task<List<UserDataContract>> GetAllUsersAsync();
    Task<List<UserDataContract>> GetUsersAsync(List<Guid> userIds);
    Task UpdateUserAsync(Guid userId, string firstName, string lastName);
    Task DeactivateUserAsync(Guid userId);
    Task<UserDataContract?> FindUserByEmailAsync(string email);
    Task<bool> UserExistsAsync(Guid userId);
}
