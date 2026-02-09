using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using TicketSystem.Issue.Domain;

namespace TicketSystem.Issue.Infrastructure.Grpc;

internal class IssueGrpcService : IssueService.IssueServiceBase
{
    private readonly Application.IssueService _issueService;

    public IssueGrpcService(Application.IssueService issueService)
    {
        _issueService = issueService;
    }

    public override async Task<CreateIssueResponse> CreateIssue(CreateIssueRequest request, ServerCallContext context)
    {
        DateTime? dueDate = null;
        if (!string.IsNullOrEmpty(request.DueDate))
        {
            dueDate = DateTime.Parse(request.DueDate).ToUniversalTime();
        }

        var issueId = await _issueService.CreateIssueAsync(request.Title, request.Description, (IssuePriority)request.Priority, dueDate);
        return new CreateIssueResponse { IssueId = issueId.ToString() };
    }

    public override async Task<GetIssueResponse> GetIssue(GetIssueRequest request, ServerCallContext context)
    {
        if (!long.TryParse(request.IssueId, out var issueId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid issue_id"));

        try
        {
            var issue = await _issueService.GetIssueAsync(issueId);
            return new GetIssueResponse { Issue = MapToMessage(issue) };
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Issue with ID {request.IssueId} not found"));
        }
    }

    public override async Task<ListIssuesResponse> ListIssues(ListIssuesRequest request, ServerCallContext context)
    {
        var issues = await _issueService.GetAllIssuesAsync();
        var response = new ListIssuesResponse();
        response.Issues.AddRange(issues.Where(i => i != null).Select(MapToMessage));
        return response;
    }

    public override async Task<UpdateIssueResponse> UpdateIssue(UpdateIssueRequest request, ServerCallContext context)
    {
        try
        {
            var issueId = long.Parse(request.IssueId);
            DateTime? dueDate = null;
            if (!string.IsNullOrEmpty(request.DueDate))
            {
                dueDate = DateTime.Parse(request.DueDate).ToUniversalTime();
            }

            await _issueService.UpdateIssueAsync(issueId, request.Title, request.Description, (IssuePriority)request.Priority, dueDate);

            return new UpdateIssueResponse();
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Issue with ID {request.IssueId} not found"));
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid issue_id"));
        }
    }

    public override async Task<AssignIssueToUserResponse> AssignIssueToUser(AssignIssueToUserRequest request, ServerCallContext context)
    {
        try
        {
            var issueId = long.Parse(request.IssueId);
            long? userId = string.IsNullOrEmpty(request.UserId) ? null : long.Parse(request.UserId);

            await _issueService.AssignIssueToUserAsync(issueId, userId);
            return new AssignIssueToUserResponse();
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

    public override async Task<AssignIssueToTeamResponse> AssignIssueToTeam(AssignIssueToTeamRequest request, ServerCallContext context)
    {
        try
        {
            var issueId = long.Parse(request.IssueId);
            long? teamId = string.IsNullOrEmpty(request.TeamId) ? null : long.Parse(request.TeamId);

            await _issueService.AssignIssueToTeamAsync(issueId, teamId);
            return new AssignIssueToTeamResponse();
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

    public override async Task<UpdateIssueStatusResponse> UpdateIssueStatus(UpdateIssueStatusRequest request, ServerCallContext context)
    {
        try
        {
            var issueId = long.Parse(request.IssueId);
            await _issueService.UpdateIssueStatusAsync(issueId, (IssueStatus)request.Status);
            return new UpdateIssueStatusResponse();
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Issue with ID {request.IssueId} not found"));
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid issue_id"));
        }
    }

    public override async Task<GetIssuesByUserResponse> GetIssuesByUser(GetIssuesByUserRequest request, ServerCallContext context)
    {
        var userId = long.Parse(request.UserId);
        var issues = await _issueService.GetIssuesByUserAsync(userId);
        var response = new GetIssuesByUserResponse();
        response.Issues.AddRange(issues.Where(i => i != null).Select(MapToMessage));
        return response;
    }

    public override async Task<GetIssuesByTeamResponse> GetIssuesByTeam(GetIssuesByTeamRequest request, ServerCallContext context)
    {
        var teamId = long.Parse(request.TeamId);
        var issues = await _issueService.GetIssuesByTeamAsync(teamId);
        var response = new GetIssuesByTeamResponse();
        response.Issues.AddRange(issues.Where(i => i != null).Select(MapToMessage));
        return response;
    }

    public override async Task<DeleteIssueResponse> DeleteIssue(DeleteIssueRequest request, ServerCallContext context)
    {
        try
        {
            var issueId = long.Parse(request.IssueId);
            await _issueService.DeleteIssueAsync(issueId);
            return new DeleteIssueResponse();
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Issue with ID {request.IssueId} not found"));
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid issue_id"));
        }
    }

    private static IssueMessage MapToMessage(IssueBusinessEntity issue)
    {
        return new IssueMessage
        {
            Id = issue.Id.ToString(),
            Title = issue.Title ?? string.Empty,
            Description = issue.Description ?? string.Empty,
            Status = (int)issue.Status,
            Priority = (int)issue.Priority,
            AssignedUserId = issue.AssignedUserId?.ToString() ?? string.Empty,
            AssignedTeamId = issue.AssignedTeamId?.ToString() ?? string.Empty,
            CreatedDate = issue.CreatedDate.ToString("O"),
            DueDate = issue.DueDate?.ToString("O") ?? string.Empty,
            ResolvedDate = issue.ResolvedDate?.ToString("O") ?? string.Empty,
            LastModifiedDate = issue.LastModifiedDate.ToString("O")
        };
    }
}
