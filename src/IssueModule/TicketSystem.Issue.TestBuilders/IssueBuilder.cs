using TicketSystem.Testing.Common.Builders;
using TicketSystem.Issue.Contracts;

namespace TicketSystem.Issue.TestBuilders;

public class IssueBuilder : BuilderBase<IssueBuilder, CreateIssueRequest>
{
    private readonly IIssueModuleApi _issueModuleApi;
    
    private string _title = $"Test Issue {Guid.NewGuid().ToString("N")[..8]}";
    private string _description = "A test issue description";
    private int _priority = 2; // Medium
    private DateTime? _dueDate = null;
    private long? _assignedUserId = null;
    private long? _assignedTeamId = null;
    private long? _createdIssueId;

    public IssueBuilder(IIssueModuleApi issueModuleApi)
    {
        _issueModuleApi = issueModuleApi;
    }
    
    public long CreatedIssueId => _createdIssueId ?? throw new InvalidOperationException("Issue has not been created yet. Call CreateAsync first.");

    public IssueBuilder WithTitle(string title)
    {
        _title = title;
        return This;
    }

    public IssueBuilder WithDescription(string description)
    {
        _description = description;
        return This;
    }

    public IssueBuilder WithHighPriority()
    {
        _priority = 1;
        return This;
    }

    public IssueBuilder WithMediumPriority()
    {
        _priority = 2;
        return This;
    }

    public IssueBuilder WithLowPriority()
    {
        _priority = 3;
        return This;
    }

    public IssueBuilder WithDueDate(DateTime dueDate)
    {
        _dueDate = dueDate;
        return This;
    }

    public IssueBuilder AssignedToUser(long userId)
    {
        _assignedUserId = userId;
        return This;
    }

    public IssueBuilder AssignedToTeam(long teamId)
    {
        _assignedTeamId = teamId;
        return This;
    }

    public override CreateIssueRequest Build()
    {
        return new CreateIssueRequest
        {
            Title = _title,
            Description = _description,
            Priority = _priority,
            DueDate = _dueDate
        };
    }

    public override async Task<CreateIssueRequest> CreateAsync()
    {
        var request = Build();
        var response = await _issueModuleApi.CreateIssueAsync(request);
        _createdIssueId = response.IssueId;
        
        // Assign to user if specified
        if (_assignedUserId.HasValue)
        {
            await _issueModuleApi.AssignIssueToUserAsync(new AssignIssueToUserRequest
            {
                IssueId = response.IssueId,
                UserId = _assignedUserId.Value
            });
        }
        
        // Assign to team if specified
        if (_assignedTeamId.HasValue)
        {
            await _issueModuleApi.AssignIssueToTeamAsync(new AssignIssueToTeamRequest
            {
                IssueId = response.IssueId,
                TeamId = _assignedTeamId.Value
            });
        }
        
        return request;
    }

    public async Task<IssueDataContract> CreateAndGetAsync()
    {
        await CreateAsync();
        var issue = await _issueModuleApi.GetIssueAsync(new GetIssueRequest { IssueId = CreatedIssueId });
        return issue ?? throw new InvalidOperationException("Issue was created but could not be retrieved");
    }
}
