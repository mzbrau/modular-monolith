namespace TicketSystem.User.Contracts;

public class GetUsersByIdsRequest
{
    public IEnumerable<long> UserIds { get; set; } = Array.Empty<long>();
}
