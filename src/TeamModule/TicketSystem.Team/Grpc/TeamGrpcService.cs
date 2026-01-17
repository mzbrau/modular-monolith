using Grpc.Core;
using TicketSystem.Team.Domain;

namespace TicketSystem.Team.Grpc;

internal class TeamGrpcService : TeamService.TeamServiceBase
{
    private readonly Application.TeamService _teamService;

    public TeamGrpcService(Application.TeamService teamService)
    {
        _teamService = teamService;
    }

    public override async Task<CreateTeamResponse> CreateTeam(CreateTeamRequest request, ServerCallContext context)
    {
        var teamId = await _teamService.CreateTeamAsync(request.Name, request.Description);
        return new CreateTeamResponse { TeamId = teamId.Value.ToString() };
    }

    public override async Task<GetTeamResponse> GetTeam(GetTeamRequest request, ServerCallContext context)
    {
        if (!long.TryParse(request.TeamId, out var teamId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid team_id"));

        try
        {
            var team = await _teamService.GetTeamAsync(new TeamId(teamId));
            return new GetTeamResponse { Team = MapToMessage(team) };
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Team with ID {request.TeamId} not found"));
        }
    }

    public override async Task<ListTeamsResponse> ListTeams(ListTeamsRequest request, ServerCallContext context)
    {
        var teams = await _teamService.GetAllTeamsAsync();
        var response = new ListTeamsResponse();
        response.Teams.AddRange(teams.Select(MapToMessage));
        return response;
    }

    public override async Task<UpdateTeamResponse> UpdateTeam(UpdateTeamRequest request, ServerCallContext context)
    {
        try
        {
            var teamId = long.Parse(request.TeamId);
            await _teamService.UpdateTeamAsync(new TeamId(teamId), request.Name, request.Description);
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
            var teamId = long.Parse(request.TeamId);
            var userId = long.Parse(request.UserId);
            await _teamService.AddMemberToTeamAsync(new TeamId(teamId), userId, (TeamRole)request.Role);
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
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID in request"));
        }
    }

    public override async Task<RemoveMemberFromTeamResponse> RemoveMemberFromTeam(RemoveMemberFromTeamRequest request, ServerCallContext context)
    {
        try
        {
            var teamId = long.Parse(request.TeamId);
            var userId = long.Parse(request.UserId);
            await _teamService.RemoveMemberFromTeamAsync(new TeamId(teamId), userId);
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
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID in request"));
        }
    }

    public override async Task<GetTeamMembersResponse> GetTeamMembers(GetTeamMembersRequest request, ServerCallContext context)
    {
        try
        {
            var teamId = long.Parse(request.TeamId);
            var members = await _teamService.GetTeamMembersAsync(new TeamId(teamId));
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

    private static TeamMessage MapToMessage(TeamBusinessEntity team)
    {
        var message = new TeamMessage
        {
            Id = team.Id.Value.ToString(),
            Name = team.Name,
            Description = team.Description ?? string.Empty,
            CreatedDate = team.CreatedDate.ToString("O")
        };
        message.Members.AddRange(team.Members.Select(MapMemberToMessage));
        return message;
    }

    private static TeamMemberMessage MapMemberToMessage(TeamMemberBusinessEntity member)
    {
        return new TeamMemberMessage
        {
            Id = member.Id.ToString(),
            UserId = member.UserId.ToString(),
            JoinedDate = member.JoinedDate.ToString("O"),
            Role = (int)member.Role
        };
    }
}
