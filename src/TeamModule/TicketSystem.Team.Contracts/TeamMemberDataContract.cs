namespace TicketSystem.Team.Contracts;

public class TeamMemberDataContract
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public DateTime JoinedDate { get; set; }
    public int Role { get; set; }
}
