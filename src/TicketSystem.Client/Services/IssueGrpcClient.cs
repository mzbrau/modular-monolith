using Grpc.Net.Client;
using TicketSystem.Issue.Grpc;

namespace TicketSystem.Client.Services;

public class IssueGrpcClient : IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly IssueService.IssueServiceClient _client;

    public IssueGrpcClient(string grpcAddress)
    {
        _channel = GrpcChannel.ForAddress(grpcAddress);
        _client = new IssueService.IssueServiceClient(_channel);
    }

    public async Task<string> CreateIssueAsync(string title, string description, int priority, DateTime? dueDate)
    {
        var response = await _client.CreateIssueAsync(new CreateIssueRequest
        {
            Title = title,
            Description = description,
            Priority = priority,
            DueDate = dueDate?.ToString("O") ?? string.Empty
        });
        return response.IssueId;
    }

    public async Task<IssueMessage?> GetIssueAsync(string issueId)
    {
        try
        {
            var response = await _client.GetIssueAsync(new GetIssueRequest { IssueId = issueId });
            return response.Issue;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<IssueMessage>> ListIssuesAsync()
    {
        var response = await _client.ListIssuesAsync(new ListIssuesRequest());
        return response.Issues.ToList();
    }

    public async Task UpdateIssueAsync(string issueId, string title, string description, int priority, DateTime? dueDate)
    {
        await _client.UpdateIssueAsync(new UpdateIssueRequest
        {
            IssueId = issueId,
            Title = title,
            Description = description,
            Priority = priority,
            DueDate = dueDate?.ToString("O") ?? string.Empty
        });
    }

    public async Task AssignIssueToUserAsync(string issueId, string? userId)
    {
        await _client.AssignIssueToUserAsync(new AssignIssueToUserRequest
        {
            IssueId = issueId,
            UserId = userId ?? string.Empty
        });
    }

    public async Task AssignIssueToTeamAsync(string issueId, string? teamId)
    {
        await _client.AssignIssueToTeamAsync(new AssignIssueToTeamRequest
        {
            IssueId = issueId,
            TeamId = teamId ?? string.Empty
        });
    }

    public async Task UpdateIssueStatusAsync(string issueId, int status)
    {
        await _client.UpdateIssueStatusAsync(new UpdateIssueStatusRequest
        {
            IssueId = issueId,
            Status = status
        });
    }

    public async Task<List<IssueMessage>> GetIssuesByUserAsync(string userId)
    {
        var response = await _client.GetIssuesByUserAsync(new GetIssuesByUserRequest { UserId = userId });
        return response.Issues.ToList();
    }

    public async Task<List<IssueMessage>> GetIssuesByTeamAsync(string teamId)
    {
        var response = await _client.GetIssuesByTeamAsync(new GetIssuesByTeamRequest { TeamId = teamId });
        return response.Issues.ToList();
    }

    public async Task DeleteIssueAsync(string issueId)
    {
        await _client.DeleteIssueAsync(new DeleteIssueRequest { IssueId = issueId });
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}
