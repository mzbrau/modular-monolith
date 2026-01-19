using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NHibernate;
using TicketSystem.User.Domain;

namespace TicketSystem.User.Infrastructure;

internal class UserRepository : IUserRepository
{
    private readonly ISession _session;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ISession session, ILogger<UserRepository> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<UserBusinessEntity?> GetByIdAsync(long id)
    {
        return await _session.GetAsync<UserBusinessEntity>(id);
    }

    public async Task<UserBusinessEntity?> GetByEmailAsync(string email)
    {
        return await _session.QueryOver<UserBusinessEntity>()
            .Where(u => u.Email == email)
            .SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyList<UserBusinessEntity>> GetAllAsync()
    {
        var users = await _session.QueryOver<UserBusinessEntity>()
            .ListAsync();
        return users.ToList();
    }

    public async Task<IReadOnlyList<UserBusinessEntity>> GetByIdsAsync(IEnumerable<long> ids)
    {
        var users = await _session.QueryOver<UserBusinessEntity>()
            .WhereRestrictionOn(u => u.Id).IsInG(ids)
            .ListAsync();
        return users.ToList();
    }

    public async Task<bool> ExistsAsync(long id)
    {
        var count = await _session.QueryOver<UserBusinessEntity>()
            .Where(u => u.Id == id)
            .RowCountAsync();
        return count > 0;
    }

    public async Task AddAsync(UserBusinessEntity user)
    {
        _logger.LogDebug("Saving new user to database: {Email}", user.Email);
        await _session.SaveAsync(user);
        _logger.LogDebug("User saved successfully with ID: {UserId}", user.Id);
    }

    public async Task UpdateAsync(UserBusinessEntity user)
    {
        // tetst
        _logger.LogDebug("Updating user in database: {UserId}", user.Id);
        // Only call Update for detached entities. If already tracked, updating can interfere
        // with NHibernate dirty-checking.
        if (!_session.Contains(user))
        {
            await _session.UpdateAsync(user);
        }

        // Some integration tests invoke module APIs directly (no HTTP transaction middleware),
        // so ensure changes are flushed to the database.
        // Only flush when there is no active transaction; otherwise the request transaction commit will flush.
        if (_session.Transaction is not { IsActive: true })
        {
            await _session.FlushAsync();
        }
        _logger.LogDebug("User updated in database: {UserId}", user.Id);
    }
}
