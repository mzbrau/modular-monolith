using TicketSystem.Client.Wpf.Models;

namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Service interface for managing users in the ticket system.
/// Provides high-level user management operations and transforms between domain models and gRPC messages.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves all users from the system.
    /// </summary>
    /// <returns>A collection of all users.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<IEnumerable<Models.User>> GetAllUsersAsync();

    /// <summary>
    /// Retrieves a specific user by their ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<Models.User?> GetUserAsync(string userId);

    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="firstName">The first name of the user.</param>
    /// <param name="lastName">The last name of the user.</param>
    /// <returns>The ID of the newly created user.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<string> CreateUserAsync(string email, string firstName, string lastName);

    /// <summary>
    /// Updates an existing user with new information.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <param name="firstName">The updated first name of the user.</param>
    /// <param name="lastName">The updated last name of the user.</param>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task UpdateUserAsync(string userId, string firstName, string lastName);

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to deactivate.</param>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task DeactivateUserAsync(string userId);

    /// <summary>
    /// Finds a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<Models.User?> FindUserByEmailAsync(string email);
}
