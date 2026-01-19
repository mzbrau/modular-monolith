using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TicketSystem.User.Application.Configuration;
using TicketSystem.User.Domain;

namespace TicketSystem.User.Application;

internal class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly IOptionsMonitor<UserSettings> _settings;

    public UserService(
        IUserRepository userRepository, 
        ILogger<UserService> logger,
        IOptionsMonitor<UserSettings> settings)
    {
        _userRepository = userRepository;
        _logger = logger;
        _settings = settings;
    }

    public async Task<long> CreateUserAsync(string email, string firstName, string lastName)
    {
        _logger.LogInformation("Creating user with email: {Email}", email);
        
        // Get current settings
        var settings = _settings.CurrentValue;
        
        // Validate email
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        
        // Validate email domain if whitelist is configured
        if (!string.IsNullOrEmpty(settings.AllowedEmailDomains))
        {
            var allowedDomains = settings.AllowedEmailDomains
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim().ToLowerInvariant())
                .ToList();
            
            var emailDomain = email.Split('@').LastOrDefault()?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(emailDomain) && !allowedDomains.Contains(emailDomain))
            {
                throw new ArgumentException($"Email domain '{emailDomain}' is not in the allowed domains list.", nameof(email));
            }
        }
        
        // Validate first name length
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        
        if (firstName.Length < settings.MinFirstNameLength)
            throw new ArgumentException($"First name must be at least {settings.MinFirstNameLength} characters long.", nameof(firstName));
        
        if (firstName.Length > settings.MaxFirstNameLength)
            throw new ArgumentException($"First name cannot exceed {settings.MaxFirstNameLength} characters.", nameof(firstName));
        
        // Validate last name length
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
        
        if (lastName.Length < settings.MinLastNameLength)
            throw new ArgumentException($"Last name must be at least {settings.MinLastNameLength} characters long.", nameof(lastName));
        
        if (lastName.Length > settings.MaxLastNameLength)
            throw new ArgumentException($"Last name cannot exceed {settings.MaxLastNameLength} characters.", nameof(lastName));
        
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
        {
            _logger.LogWarning("User creation failed - email already exists: {Email}", email);
            throw new InvalidOperationException($"User with email '{email}' already exists.");
        }

        var user = new UserBusinessEntity(0, email, firstName, lastName);
        
        await _userRepository.AddAsync(user);
        
        _logger.LogInformation("User created successfully with ID: {UserId}, Email: {Email}", user.Id, email);
        return user.Id;
    }

    public async Task<UserBusinessEntity> GetUserAsync(long userId)
    {
        _logger.LogDebug("Retrieving user with ID: {UserId}", userId);
        
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found with ID: {UserId}", userId);
            throw new KeyNotFoundException($"User with ID '{userId}' not found.");
        }
        
        _logger.LogDebug("User retrieved successfully: {UserId}", userId);
        return user;
    }

    public async Task<IReadOnlyList<UserBusinessEntity>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<IReadOnlyList<UserBusinessEntity>> GetUsersByIdsAsync(IEnumerable<long> userIds)
    {
        return await _userRepository.GetByIdsAsync(userIds);
    }

    public async Task UpdateUserAsync(long userId, string firstName, string lastName)
    {
        _logger.LogInformation("Updating user {UserId}", userId);
        
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Update failed - User not found: {UserId}", userId);
            throw new KeyNotFoundException($"User with ID '{userId}' not found.");
        }
        
        // Get current settings
        var settings = _settings.CurrentValue;
        
        // Validate first name length
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        
        if (firstName.Length < settings.MinFirstNameLength)
            throw new ArgumentException($"First name must be at least {settings.MinFirstNameLength} characters long.", nameof(firstName));
        
        if (firstName.Length > settings.MaxFirstNameLength)
            throw new ArgumentException($"First name cannot exceed {settings.MaxFirstNameLength} characters.", nameof(firstName));
        
        // Validate last name length
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
        
        if (lastName.Length < settings.MinLastNameLength)
            throw new ArgumentException($"Last name must be at least {settings.MinLastNameLength} characters long.", nameof(lastName));
        
        if (lastName.Length > settings.MaxLastNameLength)
            throw new ArgumentException($"Last name cannot exceed {settings.MaxLastNameLength} characters.", nameof(lastName));

        user.Update(firstName, lastName);
        await _userRepository.UpdateAsync(user);
        
        _logger.LogInformation("User updated successfully: {UserId}", userId);
    }

    public async Task DeactivateUserAsync(long userId)
    {
        _logger.LogInformation("Deactivating user {UserId}", userId);
        
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Deactivation failed - User not found: {UserId}", userId);
            throw new KeyNotFoundException($"User with ID '{userId}' not found.");
        }

        user.Deactivate();
        await _userRepository.UpdateAsync(user);
        
        _logger.LogInformation("User deactivated successfully: {UserId}", userId);
    }

    public async Task<UserBusinessEntity?> FindUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<bool> UserExistsAsync(long userId)
    {
        _logger.LogDebug("Checking if user exists: {UserId}", userId);
        var exists = await _userRepository.ExistsAsync(userId);
        _logger.LogDebug("User existence check result for {UserId}: {Exists}", userId, exists);
        return exists;
    }
}
