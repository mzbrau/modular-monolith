namespace TicketSystem.Team.Domain;

public class TeamBusinessEntity
{
    public virtual long Id { get; protected set; }
    public virtual string Name { get; protected set; } = string.Empty;
    public virtual string? Description { get; protected set; }
    public virtual DateTime CreatedDate { get; protected set; }
    
    private IList<TeamMemberBusinessEntity> _members = new List<TeamMemberBusinessEntity>();
    public virtual IReadOnlyList<TeamMemberBusinessEntity> Members => _members.ToList();

    protected TeamBusinessEntity() { }

    public TeamBusinessEntity(long id, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Id = id;
        Name = name;
        Description = description;
        CreatedDate = DateTime.UtcNow;
    }

    public virtual void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
        Description = description;
    }

    public virtual TeamMemberBusinessEntity AddMember(long userId, TeamRole role = TeamRole.Member)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new InvalidOperationException($"User {userId} is already a member of this team.");

        var member = new TeamMemberBusinessEntity(userId, role);
        member.SetTeam(this);
        _members.Add(member);
        return member;
    }

    public virtual void RemoveMember(long userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new InvalidOperationException($"User {userId} is not a member of this team.");

        _members.Remove(member);
    }

    public virtual bool HasMember(long userId)
    {
        return _members.Any(m => m.UserId == userId);
    }

    // For NHibernate
    protected internal virtual IList<TeamMemberBusinessEntity> MembersInternal
    {
        get => _members;
        set => _members = value ?? new List<TeamMemberBusinessEntity>();
    }
}
