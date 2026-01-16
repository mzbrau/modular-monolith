namespace TicketSystem.Issue.Domain;

public class IssueBusinessEntity
{
    public virtual IssueId Id { get; protected set; }
    public virtual string Title { get; protected set; } = string.Empty;
    public virtual string? Description { get; protected set; }
    public virtual IssueStatus Status { get; protected set; }
    public virtual IssuePriority Priority { get; protected set; }
    public virtual long? AssignedUserId { get; protected set; }
    public virtual long? AssignedTeamId { get; protected set; }
    public virtual DateTime CreatedDate { get; protected set; }
    public virtual DateTime? DueDate { get; protected set; }
    public virtual DateTime? ResolvedDate { get; protected set; }
    public virtual DateTime LastModifiedDate { get; protected set; }

    protected IssueBusinessEntity() { }

    public IssueBusinessEntity(IssueId id, string title, string? description, IssuePriority priority, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        Id = id;
        Title = title;
        Description = description;
        Status = IssueStatus.Open;
        Priority = priority;
        DueDate = dueDate;
        CreatedDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;
    }

    public virtual void Update(string title, string? description, IssuePriority priority, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        LastModifiedDate = DateTime.UtcNow;
    }

    public virtual void AssignToUser(long? userId)
    {
        AssignedUserId = userId;
        LastModifiedDate = DateTime.UtcNow;
    }

    public virtual void AssignToTeam(long? teamId)
    {
        AssignedTeamId = teamId;
        LastModifiedDate = DateTime.UtcNow;
    }

    public virtual void UpdateStatus(IssueStatus status)
    {
        var previousStatus = Status;
        Status = status;
        LastModifiedDate = DateTime.UtcNow;

        if (status == IssueStatus.Resolved && previousStatus != IssueStatus.Resolved)
        {
            ResolvedDate = DateTime.UtcNow;
        }
        else if (status != IssueStatus.Resolved && previousStatus == IssueStatus.Resolved)
        {
            ResolvedDate = null;
        }
    }
}
