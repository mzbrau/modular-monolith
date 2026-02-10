using Grpc.Core;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Team.Infrastructure.Grpc;

namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Service implementation for managing teams in the ticket system.
/// Wraps the gRPC TeamServiceClient and provides transformation between gRPC messages and domain models.
/// </summary>
public class TeamService : ITeamService
{
    private readonly TicketSystem.Team.Infrastructure.Grpc.TeamService.TeamServiceClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeamService"/> class.
    /// </summary>
    /// <param name="client">The gRPC client for team operations.</param>
    public TeamService(TicketSystem.Team.Infrastructure.Grpc.TeamService.TeamServiceClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Models.Team>> GetAllTeamsAsync()
    {
        try
        {
            var request = new ListTeamsRequest();
            var response = await _client.ListTeamsAsync(request);
            return response.Teams.Select(MapToTeam);
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
                $"Failed to retrieve teams: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Models.Team?> GetTeamAsync(string teamId)
    {
        try
        {
            var request = new GetTeamRequest { TeamId = teamId };
            var response = await _client.GetTeamAsync(request);
            return response.Team != null ? MapToTeam(response.Team) : null;
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
                $"Failed to retrieve team: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<string> CreateTeamAsync(string name, string description)
    {
        try
        {
            var request = new CreateTeamRequest
            {
                Name = name,
                Description = description
            };
            var response = await _client.CreateTeamAsync(request);
            return response.TeamId;
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
                $"Failed to create team: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task UpdateTeamAsync(string teamId, string name, string description)
    {
        try
        {
            var request = new UpdateTeamRequest
            {
                TeamId = teamId,
                Name = name,
                Description = description
            };
            await _client.UpdateTeamAsync(request);
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
            throw new InvalidOperationException($"Team not found: {teamId}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to update team: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task AddMemberToTeamAsync(string teamId, string userId, int role)
    {
        try
        {
            var request = new AddMemberToTeamRequest
            {
                TeamId = teamId,
                UserId = userId,
                Role = role
            };
            await _client.AddMemberToTeamAsync(request);
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
            throw new InvalidOperationException($"Team or user not found", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to add member to team: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveMemberFromTeamAsync(string teamId, string userId)
    {
        try
        {
            var request = new RemoveMemberFromTeamRequest
            {
                TeamId = teamId,
                UserId = userId
            };
            await _client.RemoveMemberFromTeamAsync(request);
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
            throw new InvalidOperationException($"Team or user not found", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to remove member from team: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TeamMember>> GetTeamMembersAsync(string teamId)
    {
        try
        {
            var request = new GetTeamMembersRequest { TeamId = teamId };
            var response = await _client.GetTeamMembersAsync(request);
            return response.Members.Select(MapToTeamMember);
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
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"Team not found: {teamId}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve team members: {ex.Status.Detail}", ex);
        }
    }

    /// <summary>
    /// Maps a gRPC TeamMessage to a domain Team model.
    /// </summary>
    /// <param name="message">The gRPC message to map.</param>
    /// <returns>The mapped Team domain model.</returns>
    private static Models.Team MapToTeam(TeamMessage message)
    {
        return new Models.Team
        {
            Id = message.Id,
            Name = message.Name,
            Description = message.Description,
            CreatedDate = ParseDateTime(message.CreatedDate),
            Members = message.Members.Select(MapToTeamMember).ToList()
        };
    }

    /// <summary>
    /// Maps a gRPC TeamMemberMessage to a domain TeamMember model.
    /// </summary>
    /// <param name="message">The gRPC message to map.</param>
    /// <returns>The mapped TeamMember domain model.</returns>
    private static TeamMember MapToTeamMember(TeamMemberMessage message)
    {
        return new TeamMember
        {
            Id = message.Id,
            UserId = message.UserId,
            JoinedDate = ParseDateTime(message.JoinedDate),
            Role = message.Role
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
}
