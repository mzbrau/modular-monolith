using Grpc.Core;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Issue.Infrastructure.Grpc;
using IssueModel = TicketSystem.Client.Wpf.Models.Issue;

namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Service implementation for managing issues in the ticket system.
/// Wraps the gRPC IssueServiceClient and provides transformation between gRPC messages and domain models.
/// </summary>
public class IssueService : IIssueService
{
    private readonly TicketSystem.Issue.Infrastructure.Grpc.IssueService.IssueServiceClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="IssueService"/> class.
    /// </summary>
    /// <param name="client">The gRPC client for issue operations.</param>
    public IssueService(TicketSystem.Issue.Infrastructure.Grpc.IssueService.IssueServiceClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IssueModel>> GetAllIssuesAsync()
    {
        try
        {
            var request = new ListIssuesRequest();
            var response = await _client.ListIssuesAsync(request);
            return response.Issues.Select(MapToIssue);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Invalid request: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve issues: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IssueModel?> GetIssueAsync(string issueId)
    {
        try
        {
            var request = new GetIssueRequest { IssueId = issueId };
            var response = await _client.GetIssueAsync(request);
            return response.Issue != null ? MapToIssue(response.Issue) : null;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Invalid request: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve issue: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<string> CreateIssueAsync(string title, string description, int priority, DateTime? dueDate)
    {
        try
        {
            var request = new CreateIssueRequest
            {
                Title = title,
                Description = description,
                Priority = priority,
                DueDate = dueDate?.ToString("o") ?? string.Empty
            };
            var response = await _client.CreateIssueAsync(request);
            return response.IssueId;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Validation failed: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to create issue: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task UpdateIssueAsync(string issueId, string title, string description, int priority, DateTime? dueDate)
    {
        try
        {
            var request = new UpdateIssueRequest
            {
                IssueId = issueId,
                Title = title,
                Description = description,
                Priority = priority,
                DueDate = dueDate?.ToString("o") ?? string.Empty
            };
            await _client.UpdateIssueAsync(request);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Validation failed: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"Issue not found: {issueId}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to update issue: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task UpdateIssueStatusAsync(string issueId, IssueStatus status)
    {
        try
        {
            var request = new UpdateIssueStatusRequest
            {
                IssueId = issueId,
                Status = (int)status
            };
            await _client.UpdateIssueStatusAsync(request);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Validation failed: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"Issue not found: {issueId}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to update issue status: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task AssignIssueToUserAsync(string issueId, string userId)
    {
        try
        {
            var request = new AssignIssueToUserRequest
            {
                IssueId = issueId,
                UserId = userId
            };
            await _client.AssignIssueToUserAsync(request);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Validation failed: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"Issue or user not found", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to assign issue to user: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task AssignIssueToTeamAsync(string issueId, string teamId)
    {
        try
        {
            var request = new AssignIssueToTeamRequest
            {
                IssueId = issueId,
                TeamId = teamId
            };
            await _client.AssignIssueToTeamAsync(request);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Validation failed: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"Issue or team not found", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to assign issue to team: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IssueModel>> GetIssuesByUserAsync(string userId)
    {
        try
        {
            var request = new GetIssuesByUserRequest { UserId = userId };
            var response = await _client.GetIssuesByUserAsync(request);
            return response.Issues.Select(MapToIssue);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Invalid request: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve issues by user: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IssueModel>> GetIssuesByTeamAsync(string teamId)
    {
        try
        {
            var request = new GetIssuesByTeamRequest { TeamId = teamId };
            var response = await _client.GetIssuesByTeamAsync(request);
            return response.Issues.Select(MapToIssue);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Invalid request: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve issues by team: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteIssueAsync(string issueId)
    {
        try
        {
            var request = new DeleteIssueRequest { IssueId = issueId };
            await _client.DeleteIssueAsync(request);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"Issue not found: {issueId}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to delete issue: {ex.Status.Detail}", ex);
        }
    }

    /// <summary>
    /// Maps a gRPC IssueMessage to a domain Issue model.
    /// </summary>
    /// <param name="message">The gRPC message to map.</param>
    /// <returns>The mapped Issue domain model.</returns>
    private static IssueModel MapToIssue(IssueMessage message)
    {
        return new IssueModel
        {
            Id = message.Id,
            Title = message.Title,
            Description = message.Description,
            Status = (IssueStatus)message.Status,
            Priority = message.Priority,
            AssignedUserId = string.IsNullOrEmpty(message.AssignedUserId) ? null : message.AssignedUserId,
            AssignedTeamId = string.IsNullOrEmpty(message.AssignedTeamId) ? null : message.AssignedTeamId,
            CreatedDate = ParseDateTime(message.CreatedDate),
            DueDate = ParseNullableDateTime(message.DueDate),
            ResolvedDate = ParseNullableDateTime(message.ResolvedDate),
            LastModifiedDate = ParseDateTime(message.LastModifiedDate)
        };
    }

    /// <summary>
    /// Parses a date string to a DateTime object.
    /// </summary>
    /// <param name="dateString">The date string to parse.</param>
    /// <returns>The parsed DateTime.</returns>
    private static DateTime ParseDateTime(string dateString)
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return DateTime.MinValue;
        }

        if (DateTime.TryParse(dateString, out var result))
        {
            return result;
        }

        return DateTime.MinValue;
    }

    /// <summary>
    /// Parses a date string to a nullable DateTime object.
    /// </summary>
    /// <param name="dateString">The date string to parse.</param>
    /// <returns>The parsed DateTime, or null if the string is empty or invalid.</returns>
    private static DateTime? ParseNullableDateTime(string dateString)
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return null;
        }

        if (DateTime.TryParse(dateString, out var result))
        {
            return result;
        }

        return null;
    }
}
