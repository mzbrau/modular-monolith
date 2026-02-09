using System;

namespace TicketSystem.Team.Domain;

public class TeamMemberBusinessEntity
{
    public virtual long Id { get; protected set; }
    public virtual long UserId { get; protected set; }
    public virtual DateTime JoinedDate { get; protected set; }
    public virtual TeamRole Role { get; protected set; }
    public virtual TeamBusinessEntity Team { get; protected set; } = null!;

    protected TeamMemberBusinessEntity() { }

    public TeamMemberBusinessEntity(long userId, TeamRole role)
    {
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
