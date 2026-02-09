namespace TicketSystem.Team.Contracts;

public class AddMemberToTeamRequest
{
    public long TeamId { get; set; }
    public long UserId { get; set; }
    public int Role { get; set; }
}
