namespace TicketSystem.Team.Contracts;

public class RemoveMemberFromTeamRequest
{
    public long TeamId { get; set; }
    public long UserId { get; set; }
}
