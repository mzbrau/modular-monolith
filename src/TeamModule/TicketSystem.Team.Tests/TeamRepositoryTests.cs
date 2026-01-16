using NHibernate;
using NSubstitute;
using TicketSystem.Team.Domain;
using TicketSystem.Team.Infrastructure;

namespace TicketSystem.TeamModule.Tests;

[TestFixture]
public class TeamRepositoryTests
{
    [Test]
    public async Task GetByIdAsync_PassesStronglyTypedIdToNHibernate()
    {
        var session = Substitute.For<ISession>();
        var repository = new TeamRepository(session);
        var teamId = new TeamId(1);

        await repository.GetByIdAsync(teamId);

        await session.Received(1).GetAsync<TeamBusinessEntity>(teamId, Arg.Any<CancellationToken>());
    }
}
