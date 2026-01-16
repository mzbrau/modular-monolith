namespace TicketSystem.User.Contracts;

public interface IUserModuleApi
{
    Task<long> CreateUserAsync(string email, string firstName, string lastName);
    Task<UserDataContract?> GetUserAsync(long userId);
    Task<List<UserDataContract>> GetAllUsersAsync();
    Task<List<UserDataContract>> GetUsersAsync(List<long> userIds);
    Task UpdateUserAsync(long userId, string firstName, string lastName);
    Task DeactivateUserAsync(long userId);
    Task<UserDataContract?> FindUserByEmailAsync(string email);
    Task<bool> UserExistsAsync(long userId);
}
