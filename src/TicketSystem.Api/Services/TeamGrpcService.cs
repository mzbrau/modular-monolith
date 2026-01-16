using Grpc.Core;
using TicketSystem.Api.Protos;
using TicketSystem.Team.Contracts;

namespace TicketSystem.Api.Services;

public class TeamGrpcService : TeamService.TeamServiceBase
{
    private readonly ITeamModuleApi _teamModuleApi;

    public TeamGrpcService(ITeamModuleApi teamModuleApi)
    {
        _teamModuleApi = teamModuleApi;
    }

    public override async Task<CreateTeamResponse> CreateTeam(CreateTeamRequest request, ServerCallContext context)
    {
        var teamId = await _teamModuleApi.CreateTeamAsync(request.Name, request.Description);
        return new CreateTeamResponse { TeamId = teamId.ToString() };
    }

    public override async Task<GetTeamResponse> GetTeam(GetTeamRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.TeamId, out var teamId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid team_id"));

        var team = await _teamModuleApi.GetTeamAsync(teamId);
        if (team == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Team with ID {request.TeamId} not found"));

        return new GetTeamResponse { Team = MapToMessage(team) };
    }

    public override async Task<ListTeamsResponse> ListTeams(ListTeamsRequest request, ServerCallContext context)
    {
        var teams = await _teamModuleApi.GetAllTeamsAsync();
        var response = new ListTeamsResponse();
        response.Teams.AddRange(teams.Select(MapToMessage));
        return response;
    }

    public override async Task<UpdateTeamResponse> UpdateTeam(UpdateTeamRequest request, ServerCallContext context)
    {
        try
        {
            var teamId = Guid.Parse(request.TeamId);
            await _teamModuleApi.UpdateTeamAsync(teamId, request.Name, request.Description);
            return new UpdateTeamResponse();
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Team with ID {request.TeamId} not found"));
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid team_id"));
        }
    }

    public override async Task<AddMemberToTeamResponse> AddMemberToTeam(AddMemberToTeamRequest request, ServerCallContext context)
    {
        try
        {
            var teamId = Guid.Parse(request.TeamId);
            var userId = Guid.Parse(request.UserId);
            await _teamModuleApi.AddMemberToTeamAsync(teamId, userId, request.Role);
            return new AddMemberToTeamResponse();
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid GUID in request"));
        }
    }

    public override async Task<RemoveMemberFromTeamResponse> RemoveMemberFromTeam(RemoveMemberFromTeamRequest request, ServerCallContext context)
    {
        try
        {
            var teamId = Guid.Parse(request.TeamId);
            var userId = Guid.Parse(request.UserId);
            await _teamModuleApi.RemoveMemberFromTeamAsync(teamId, userId);
            return new RemoveMemberFromTeamResponse();
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid GUID in request"));
        }
    }

    public override async Task<GetTeamMembersResponse> GetTeamMembers(GetTeamMembersRequest request, ServerCallContext context)
    {
        try
        {
            var teamId = Guid.Parse(request.TeamId);
            var members = await _teamModuleApi.GetTeamMembersAsync(teamId);
            var response = new GetTeamMembersResponse();
            response.Members.AddRange(members.Select(MapMemberToMessage));
            return response;
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Team with ID {request.TeamId} not found"));
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid team_id"));
        }
    }

    private static TeamMessage MapToMessage(TeamDataContract team)
    {
        var message = new TeamMessage
        {
            Id = team.Id.ToString(),
            Name = team.Name,
            Description = team.Description ?? string.Empty,
            CreatedDate = team.CreatedDate.ToString("O")
        };
        message.Members.AddRange(team.Members.Select(MapMemberToMessage));
        return message;
    }

    private static TeamMemberMessage MapMemberToMessage(TeamMemberDataContract member)
    {
        return new TeamMemberMessage
        {
            Id = member.Id.ToString(),
            UserId = member.UserId.ToString(),
            JoinedDate = member.JoinedDate.ToString("O"),
            Role = member.Role
        };
    }
}
