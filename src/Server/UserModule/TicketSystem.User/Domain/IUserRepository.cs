using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicketSystem.User.Domain;

internal interface IUserRepository
{
    Task<UserBusinessEntity?> GetByIdAsync(long id);
    Task<UserBusinessEntity?> GetByEmailAsync(string email);
    Task<IReadOnlyList<UserBusinessEntity>> GetAllAsync();
    Task<IReadOnlyList<UserBusinessEntity>> GetByIdsAsync(IEnumerable<long> ids);
    Task<bool> ExistsAsync(long id);
    Task AddAsync(UserBusinessEntity user);
    Task UpdateAsync(UserBusinessEntity user);
}
