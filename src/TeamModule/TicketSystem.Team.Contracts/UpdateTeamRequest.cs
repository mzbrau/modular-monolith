namespace TicketSystem.Team.Contracts;

public class UpdateTeamRequest
{
    public long TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
