namespace TicketSystem.Team.Domain;

public class TeamMemberBusinessEntity
{
    public virtual Guid Id { get; protected set; }
    public virtual Guid UserId { get; protected set; }
    public virtual DateTime JoinedDate { get; protected set; }
    public virtual TeamRole Role { get; protected set; }
    public virtual TeamBusinessEntity Team { get; protected set; } = null!;

    protected TeamMemberBusinessEntity() { }

    public TeamMemberBusinessEntity(Guid userId, TeamRole role)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        JoinedDate = DateTime.UtcNow;
        Role = role;
    }

    internal void SetTeam(TeamBusinessEntity team)
    {
        Team = team;
    }

    public virtual void UpdateRole(TeamRole role)
    {
        Role = role;
    }
}
