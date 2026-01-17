using Grpc.Net.Client;
using TicketSystem.Team.Infrastructure.Grpc;

namespace TicketSystem.Client.Services;

public class TeamGrpcClient : IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly TeamService.TeamServiceClient _client;

    public TeamGrpcClient(string grpcAddress)
    {
        _channel = GrpcChannel.ForAddress(grpcAddress);
        _client = new TeamService.TeamServiceClient(_channel);
    }

    public async Task<string> CreateTeamAsync(string name, string description)
    {
        var response = await _client.CreateTeamAsync(new CreateTeamRequest
        {
            Name = name,
            Description = description
        });
        return response.TeamId;
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
        await _client.AddMemberToTeamAsync(new AddMemberToTeamRequest
        {
            TeamId = teamId,
            UserId = userId,
            Role = role
        });
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
