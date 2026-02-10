using TicketSystem.Client.Wpf.Models;

namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Service interface for managing teams in the ticket system.
/// Provides high-level team management operations and transforms between domain models and gRPC messages.
/// </summary>
public interface ITeamService
{
    /// <summary>
    /// Retrieves all teams from the system.
    /// </summary>
    /// <returns>A collection of all teams.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<IEnumerable<Models.Team>> GetAllTeamsAsync();

    /// <summary>
    /// Retrieves a specific team by its ID.
    /// </summary>
    /// <param name="teamId">The unique identifier of the team.</param>
    /// <returns>The team if found; otherwise, null.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<Models.Team?> GetTeamAsync(string teamId);

    /// <summary>
    /// Creates a new team in the system.
    /// </summary>
    /// <param name="name">The name of the team.</param>
    /// <param name="description">The description of the team.</param>
    /// <returns>The ID of the newly created team.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<string> CreateTeamAsync(string name, string description);

    /// <summary>
    /// Updates an existing team with new information.
    /// </summary>
    /// <param name="teamId">The unique identifier of the team to update.</param>
    /// <param name="name">The updated name of the team.</param>
    /// <param name="description">The updated description of the team.</param>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task UpdateTeamAsync(string teamId, string name, string description);

    /// <summary>
    /// Adds a member to a team with a specific role.
    /// </summary>
    /// <param name="teamId">The unique identifier of the team.</param>
    /// <param name="userId">The unique identifier of the user to add.</param>
    /// <param name="role">The role of the member within the team.</param>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task AddMemberToTeamAsync(string teamId, string userId, int role);

    /// <summary>
    /// Removes a member from a team.
    /// </summary>
    /// <param name="teamId">The unique identifier of the team.</param>
    /// <param name="userId">The unique identifier of the user to remove.</param>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task RemoveMemberFromTeamAsync(string teamId, string userId);

    /// <summary>
    /// Retrieves all members of a specific team.
    /// </summary>
    /// <param name="teamId">The unique identifier of the team.</param>
    /// <returns>A collection of team members.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<IEnumerable<TeamMember>> GetTeamMembersAsync(string teamId);
}
