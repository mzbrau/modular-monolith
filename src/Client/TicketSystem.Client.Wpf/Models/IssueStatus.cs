namespace TicketSystem.Client.Wpf.Models;

/// <summary>
/// Represents the status of an issue in the ticket system.
/// </summary>
public enum IssueStatus
{
    /// <summary>
    /// Issue is newly created and not yet started.
    /// </summary>
    Open = 0,

    /// <summary>
    /// Issue is currently being worked on.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Issue is blocked and cannot proceed.
    /// </summary>
    Blocked = 2,

    /// <summary>
    /// Issue has been resolved.
    /// </summary>
    Resolved = 3,

    /// <summary>
    /// Issue is closed and no further action is needed.
    /// </summary>
    Closed = 4
}
