namespace TicketSystem.Team.Contracts;

public class TeamDataContract
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<TeamMemberDataContract> Members { get; set; } = new();
}
