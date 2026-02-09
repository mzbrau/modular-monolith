namespace TicketSystem.User.Contracts;

public class UpdateUserRequest
{
    public long UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
