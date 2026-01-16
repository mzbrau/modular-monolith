namespace TicketSystem.Team.Contracts;

public class TeamMemberDataContract
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedDate { get; set; }
    public int Role { get; set; }
}
