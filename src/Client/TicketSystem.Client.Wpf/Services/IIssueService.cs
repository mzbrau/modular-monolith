using TicketSystem.Client.Wpf.Models;
using IssueModel = TicketSystem.Client.Wpf.Models.Issue;

namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Service interface for managing issues in the ticket system.
/// Provides high-level issue management operations and transforms between domain models and gRPC messages.
/// </summary>
public interface IIssueService
{
    /// <summary>
    /// Retrieves all issues from the system.
    /// </summary>
    /// <returns>A collection of all issues.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<IEnumerable<IssueModel>> GetAllIssuesAsync();

    /// <summary>
    /// Retrieves a specific issue by its ID.
    /// </summary>
    /// <param name="issueId">The unique identifier of the issue.</param>
    /// <returns>The issue if found; otherwise, null.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<IssueModel?> GetIssueAsync(string issueId);

    /// <summary>
    /// Creates a new issue in the system.
    /// </summary>
    /// <param name="title">The title of the issue.</param>
    /// <param name="description">The description of the issue.</param>
    /// <param name="priority">The priority level of the issue.</param>
    /// <param name="dueDate">The optional due date for the issue.</param>
    /// <returns>The ID of the newly created issue.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<string> CreateIssueAsync(string title, string description, int priority, DateTime? dueDate);

    /// <summary>
    /// Updates an existing issue with new information.
    /// </summary>
    /// <param name="issueId">The unique identifier of the issue to update.</param>
    /// <param name="title">The updated title of the issue.</param>
    /// <param name="description">The updated description of the issue.</param>
    /// <param name="priority">The updated priority level of the issue.</param>
    /// <param name="dueDate">The updated due date for the issue.</param>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task UpdateIssueAsync(string issueId, string title, string description, int priority, DateTime? dueDate);

    /// <summary>
    /// Updates the status of an issue.
    /// </summary>
    /// <param name="issueId">The unique identifier of the issue.</param>
    /// <param name="status">The new status for the issue.</param>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task UpdateIssueStatusAsync(string issueId, IssueStatus status);

    /// <summary>
    /// Assigns an issue to a specific user.
    /// </summary>
    /// <param name="issueId">The unique identifier of the issue.</param>
    /// <param name="userId">The unique identifier of the user to assign the issue to.</param>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task AssignIssueToUserAsync(string issueId, string userId);

    /// <summary>
    /// Assigns an issue to a specific team.
    /// </summary>
    /// <param name="issueId">The unique identifier of the issue.</param>
    /// <param name="teamId">The unique identifier of the team to assign the issue to.</param>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task AssignIssueToTeamAsync(string issueId, string teamId);

    /// <summary>
    /// Retrieves all issues assigned to a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of issues assigned to the user.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<IEnumerable<IssueModel>> GetIssuesByUserAsync(string userId);

    /// <summary>
    /// Retrieves all issues assigned to a specific team.
    /// </summary>
    /// <param name="teamId">The unique identifier of the team.</param>
    /// <returns>A collection of issues assigned to the team.</returns>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task<IEnumerable<IssueModel>> GetIssuesByTeamAsync(string teamId);

    /// <summary>
    /// Deletes an issue from the system.
    /// </summary>
    /// <param name="issueId">The unique identifier of the issue to delete.</param>
    /// <exception cref="Grpc.Core.RpcException">Thrown when the gRPC call fails.</exception>
    Task DeleteIssueAsync(string issueId);
}
