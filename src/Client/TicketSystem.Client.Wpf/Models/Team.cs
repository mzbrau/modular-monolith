namespace TicketSystem.Client.Wpf.Models;

/// <summary>
/// Represents a team that can be assigned to issues collectively.
/// </summary>
public class Team
{
    /// <summary>
    /// Gets or sets the unique identifier for the team.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the team.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the team.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the team was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the list of members belonging to this team.
    /// </summary>
    public List<TeamMember> Members { get; set; } = new();
}
