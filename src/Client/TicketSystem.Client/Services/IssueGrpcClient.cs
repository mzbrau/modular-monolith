using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using TicketSystem.Issue.Infrastructure.Grpc;

namespace TicketSystem.Client.Services;

public class IssueGrpcClient : IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly IssueService.IssueServiceClient _client;
    private readonly ILogger<IssueGrpcClient> _logger;

    public IssueGrpcClient(string grpcAddress, ILogger<IssueGrpcClient> logger)
    {
        _channel = GrpcChannel.ForAddress(grpcAddress);
        _client = new IssueService.IssueServiceClient(_channel);
        _logger = logger;
        _logger.LogInformation("IssueGrpcClient initialized with address: {GrpcAddress}", grpcAddress);
    }

    public async Task<string> CreateIssueAsync(string title, string description, int priority, DateTime? dueDate)
    {
        _logger.LogInformation("Creating issue via gRPC: {Title}, Priority: {Priority}", title, priority);
        try
        {
            var response = await _client.CreateIssueAsync(new CreateIssueRequest
            {
                Title = title,
                Description = description,
                Priority = priority,
                DueDate = dueDate?.ToString("O") ?? string.Empty
            });
            _logger.LogInformation("Issue created successfully via gRPC: {IssueId}", response.IssueId);
            return response.IssueId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create issue via gRPC: {Title}", title);
            throw;
        }
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
        _logger.LogInformation("Assigning issue {IssueId} to user {UserId} via gRPC", issueId, userId ?? "(unassigned)");
        try
        {
            await _client.AssignIssueToUserAsync(new AssignIssueToUserRequest
            {
                IssueId = issueId,
                UserId = userId ?? string.Empty
            });
            _logger.LogInformation("Issue {IssueId} assigned to user successfully", issueId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign issue {IssueId} to user via gRPC", issueId);
            throw;
        }
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
