using TicketSystem.Issue.Contracts;

namespace TicketSystem.Issue.TestBuilders;

/// <summary>
/// Factory class for creating IssueBuilders within a test context.
/// </summary>
public class IssueTestDataSeeder
{
    private readonly IIssueModuleApi _issueModuleApi;

    public IssueTestDataSeeder(IIssueModuleApi issueModuleApi)
    {
        _issueModuleApi = issueModuleApi;
    }

    public IssueBuilder AnIssue() => new IssueBuilder(_issueModuleApi);

    public IssueBuilder ABug() => new IssueBuilder(_issueModuleApi)
        .WithTitle("Bug")
        .WithDescription("A bug that needs fixing");

    public IssueBuilder ACriticalBug() => new IssueBuilder(_issueModuleApi)
        .WithTitle("Critical Bug")
        .WithDescription("A critical bug that needs immediate attention")
        .WithHighPriority();

    /// <summary>
    /// Creates a standard test issue with default values.
    /// </summary>
    public async Task<IssueDataContract> CreateStandardIssueAsync()
    {
        return await AnIssue().CreateAndGetAsync();
    }
}
