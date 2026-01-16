using TicketSystem.Team.Contracts;
using TicketSystem.Team.Domain;

namespace TicketSystem.Team.Adapters;

internal static class TeamConverter
{
    public static TeamDataContract ToDataContract(TeamBusinessEntity team)
    {
        return new TeamDataContract
        {
            Id = team.Id.Value,
            Name = team.Name,
            Description = team.Description,
            CreatedDate = team.CreatedDate,
            Members = team.Members.Select(m => new TeamMemberDataContract
            {
                Id = m.Id,
                UserId = m.UserId,
                Role = (int)m.Role,
                JoinedDate = m.JoinedDate
            }).ToList()
        };
    }
}
