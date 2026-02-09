using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using TicketSystem.Team.Infrastructure.Grpc;

namespace TicketSystem.Client.Services;

public class TeamGrpcClient : IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly TeamService.TeamServiceClient _client;
    private readonly ILogger<TeamGrpcClient> _logger;

    public TeamGrpcClient(string grpcAddress, ILogger<TeamGrpcClient> logger)
    {
        _channel = GrpcChannel.ForAddress(grpcAddress);
        _client = new TeamService.TeamServiceClient(_channel);
        _logger = logger;
        _logger.LogInformation("TeamGrpcClient initialized with address: {GrpcAddress}", grpcAddress);
    }

    public async Task<string> CreateTeamAsync(string name, string description)
    {
        _logger.LogInformation("Creating team via gRPC: {TeamName}", name);
        try
        {
            var response = await _client.CreateTeamAsync(new CreateTeamRequest
            {
                Name = name,
                Description = description
            });
            _logger.LogInformation("Team created successfully via gRPC: {TeamId}", response.TeamId);
            return response.TeamId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create team via gRPC: {TeamName}", name);
            throw;
        }
    }

    public async Task<TeamMessage?> GetTeamAsync(string teamId)
    {
        try
        {
            var response = await _client.GetTeamAsync(new GetTeamRequest { TeamId = teamId });
            return response.Team;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<TeamMessage>> ListTeamsAsync()
    {
        var response = await _client.ListTeamsAsync(new ListTeamsRequest());
        return response.Teams.ToList();
    }

    public async Task UpdateTeamAsync(string teamId, string name, string description)
    {
        await _client.UpdateTeamAsync(new UpdateTeamRequest
        {
            TeamId = teamId,
            Name = name,
            Description = description
        });
    }

    public async Task AddMemberToTeamAsync(string teamId, string userId, int role)
    {
        _logger.LogInformation("Adding user {UserId} to team {TeamId} via gRPC", userId, teamId);
        try
        {
            await _client.AddMemberToTeamAsync(new AddMemberToTeamRequest
            {
                TeamId = teamId,
                UserId = userId,
                Role = role
            });
            _logger.LogInformation("User {UserId} added to team {TeamId} successfully", userId, teamId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add user {UserId} to team {TeamId} via gRPC", userId, teamId);
            throw;
        }
    }

    public async Task RemoveMemberFromTeamAsync(string teamId, string userId)
    {
        await _client.RemoveMemberFromTeamAsync(new RemoveMemberFromTeamRequest
        {
            TeamId = teamId,
            UserId = userId
        });
    }

    public async Task<List<TeamMemberMessage>> GetTeamMembersAsync(string teamId)
    {
        var response = await _client.GetTeamMembersAsync(new GetTeamMembersRequest { TeamId = teamId });
        return response.Members.ToList();
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}
