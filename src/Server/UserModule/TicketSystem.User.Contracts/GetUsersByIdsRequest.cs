using System;
using System.Collections.Generic;

namespace TicketSystem.User.Contracts;

public class GetUsersByIdsRequest
{
    public IEnumerable<long> UserIds { get; set; } = [];
}
