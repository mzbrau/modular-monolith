namespace TicketSystem.Client.Wpf.Models;

/// <summary>
/// Represents a member of a team with their role and membership details.
/// </summary>
public class TeamMember
{
    /// <summary>
    /// Gets or sets the unique identifier for the team member.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the user who is a member of the team.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the user joined the team.
    /// </summary>
    public DateTime JoinedDate { get; set; }

    /// <summary>
    /// Gets or sets the role of the team member (permission level within the team).
    /// </summary>
    public int Role { get; set; }
}
