using Grpc.Core;
using Grpc.Core.Testing;
using Moq;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Team.Infrastructure.Grpc;
using TeamModel = TicketSystem.Client.Wpf.Models.Team;

namespace TicketSystem.Client.Wpf.Tests.Services;

/// <summary>
/// Unit tests for TeamService.
/// Tests model transformation from TeamMessage to Team and error handling scenarios.
/// </summary>
public class TeamServiceTests
{
    private readonly Mock<TicketSystem.Team.Infrastructure.Grpc.TeamService.TeamServiceClient> _mockClient;
    private readonly TicketSystem.Client.Wpf.Services.TeamService _service;

    public TeamServiceTests()
    {
        _mockClient = new Mock<TicketSystem.Team.Infrastructure.Grpc.TeamService.TeamServiceClient>();
        _service = new TicketSystem.Client.Wpf.Services.TeamService(_mockClient.Object);
    }

    private static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
    {
        return TestCalls.AsyncUnaryCall(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }

    private static AsyncUnaryCall<TResponse> CreateAsyncUnaryCallWithException<TResponse>(RpcException exception)
    {
        return TestCalls.AsyncUnaryCall<TResponse>(
            Task.FromException<TResponse>(exception),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }

    #region Model Transformation Tests

    [Fact]
    public async Task GetAllTeamsAsync_TransformsTeamMessageToTeam_Correctly()
    {
        // Arrange
        var teamMessage = new TeamMessage
        {
            Id = "team-123",
            Name = "Development Team",
            Description = "Main development team",
            CreatedDate = "2024-01-15T10:30:00Z"
        };
        teamMessage.Members.Add(new TeamMemberMessage
        {
            Id = "member-1",
            UserId = "user-456",
            JoinedDate = "2024-01-16T10:30:00Z",
            Role = 1
        });

        var response = new ListTeamsResponse();
        response.Teams.Add(teamMessage);

        _mockClient.Setup(c => c.ListTeamsAsync(
            It.IsAny<ListTeamsRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.GetAllTeamsAsync();
        var team = result.First();

        // Assert
        Assert.Equal("team-123", team.Id);
        Assert.Equal("Development Team", team.Name);
        Assert.Equal("Main development team", team.Description);
        Assert.NotEqual(DateTime.MinValue, team.CreatedDate);
        Assert.Single(team.Members);
        Assert.Equal("member-1", team.Members[0].Id);
        Assert.Equal("user-456", team.Members[0].UserId);
        Assert.Equal(1, team.Members[0].Role);
    }

    [Fact]
    public async Task GetTeamAsync_TransformsTeamMessageWithEmptyMembers_Correctly()
    {
        // Arrange
        var teamMessage = new TeamMessage
        {
            Id = "team-123",
            Name = "Empty Team",
            Description = "Team with no members",
            CreatedDate = "2024-01-15T10:30:00Z"
        };

        var response = new GetTeamResponse { Team = teamMessage };

        _mockClient.Setup(c => c.GetTeamAsync(
            It.IsAny<GetTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.GetTeamAsync("team-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("team-123", result.Id);
        Assert.Empty(result.Members);
    }

    [Fact]
    public async Task GetTeamMembersAsync_TransformsTeamMemberMessages_Correctly()
    {
        // Arrange
        var response = new GetTeamMembersResponse();
        response.Members.Add(new TeamMemberMessage
        {
            Id = "member-1",
            UserId = "user-1",
            JoinedDate = "2024-01-15T10:30:00Z",
            Role = 1
        });
        response.Members.Add(new TeamMemberMessage
        {
            Id = "member-2",
            UserId = "user-2",
            JoinedDate = "2024-01-16T10:30:00Z",
            Role = 2
        });

        _mockClient.Setup(c => c.GetTeamMembersAsync(
            It.IsAny<GetTeamMembersRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.GetTeamMembersAsync("team-123");
        var members = result.ToList();

        // Assert
        Assert.Equal(2, members.Count);
        Assert.Equal("member-1", members[0].Id);
        Assert.Equal("user-1", members[0].UserId);
        Assert.Equal(1, members[0].Role);
        Assert.Equal("member-2", members[1].Id);
        Assert.Equal("user-2", members[1].UserId);
        Assert.Equal(2, members[1].Role);
    }

    #endregion

    #region Network Error Tests

    [Fact]
    public async Task GetAllTeamsAsync_WhenUnavailable_ThrowsInvalidOperationExceptionWithNetworkMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Unavailable, "Service unavailable"));
        _mockClient.Setup(c => c.ListTeamsAsync(
            It.IsAny<ListTeamsRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<ListTeamsResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetAllTeamsAsync());
        Assert.Contains("Unable to connect to the server", exception.Message);
    }

    [Fact]
    public async Task CreateTeamAsync_WhenDeadlineExceeded_ThrowsInvalidOperationExceptionWithNetworkMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline exceeded"));
        _mockClient.Setup(c => c.CreateTeamAsync(
            It.IsAny<CreateTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<CreateTeamResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateTeamAsync("Test Team", "Description"));
        Assert.Contains("Unable to connect to the server", exception.Message);
    }

    [Fact]
    public async Task UpdateTeamAsync_WhenCancelled_ThrowsInvalidOperationExceptionWithNetworkMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Cancelled, "Request cancelled"));
        _mockClient.Setup(c => c.UpdateTeamAsync(
            It.IsAny<UpdateTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<UpdateTeamResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateTeamAsync("team-123", "Updated Name", "Updated Description"));
        Assert.Contains("Unable to connect to the server", exception.Message);
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task CreateTeamAsync_WhenInvalidArgument_ThrowsInvalidOperationExceptionWithValidationMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.InvalidArgument, "Name is required"));
        _mockClient.Setup(c => c.CreateTeamAsync(
            It.IsAny<CreateTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<CreateTeamResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateTeamAsync("", "Description"));
        Assert.Contains("Validation failed", exception.Message);
        Assert.Contains("Name is required", exception.Message);
    }

    [Fact]
    public async Task AddMemberToTeamAsync_WhenFailedPrecondition_ThrowsInvalidOperationExceptionWithValidationMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.FailedPrecondition, "User already in team"));
        _mockClient.Setup(c => c.AddMemberToTeamAsync(
            It.IsAny<AddMemberToTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<AddMemberToTeamResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddMemberToTeamAsync("team-123", "user-456", 1));
        Assert.Contains("Validation failed", exception.Message);
        Assert.Contains("User already in team", exception.Message);
    }

    #endregion

    #region NotFound Error Tests

    [Fact]
    public async Task GetTeamAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "Team not found"));
        _mockClient.Setup(c => c.GetTeamAsync(
            It.IsAny<GetTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<GetTeamResponse>(rpcException));

        // Act
        var result = await _service.GetTeamAsync("nonexistent-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTeamAsync_WhenNotFound_ThrowsInvalidOperationExceptionWithNotFoundMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "Team not found"));
        _mockClient.Setup(c => c.UpdateTeamAsync(
            It.IsAny<UpdateTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<UpdateTeamResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateTeamAsync("nonexistent-id", "Name", "Description"));
        Assert.Contains("Team not found", exception.Message);
    }

    [Fact]
    public async Task GetTeamMembersAsync_WhenNotFound_ThrowsInvalidOperationExceptionWithNotFoundMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "Team not found"));
        _mockClient.Setup(c => c.GetTeamMembersAsync(
            It.IsAny<GetTeamMembersRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<GetTeamMembersResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetTeamMembersAsync("nonexistent-id"));
        Assert.Contains("Team not found", exception.Message);
    }

    #endregion

    #region Unexpected Error Tests

    [Fact]
    public async Task GetAllTeamsAsync_WhenUnexpectedError_ThrowsInvalidOperationExceptionWithGenericMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        _mockClient.Setup(c => c.ListTeamsAsync(
            It.IsAny<ListTeamsRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<ListTeamsResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetAllTeamsAsync());
        Assert.Contains("Failed to retrieve teams", exception.Message);
    }

    [Fact]
    public async Task CreateTeamAsync_WhenUnexpectedError_ThrowsInvalidOperationExceptionWithGenericMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Unknown, "Unknown error"));
        _mockClient.Setup(c => c.CreateTeamAsync(
            It.IsAny<CreateTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<CreateTeamResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateTeamAsync("Test Team", "Description"));
        Assert.Contains("Failed to create team", exception.Message);
    }

    #endregion

    #region Successful Operation Tests

    [Fact]
    public async Task CreateTeamAsync_WhenSuccessful_ReturnsTeamId()
    {
        // Arrange
        var response = new CreateTeamResponse { TeamId = "new-team-123" };
        _mockClient.Setup(c => c.CreateTeamAsync(
            It.IsAny<CreateTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.CreateTeamAsync("Development Team", "Main development team");

        // Assert
        Assert.Equal("new-team-123", result);
    }

    [Fact]
    public async Task UpdateTeamAsync_WhenSuccessful_CompletesWithoutException()
    {
        // Arrange
        var response = new UpdateTeamResponse();
        _mockClient.Setup(c => c.UpdateTeamAsync(
            It.IsAny<UpdateTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act & Assert - Should not throw
        await _service.UpdateTeamAsync("team-123", "Updated Name", "Updated Description");
    }

    [Fact]
    public async Task AddMemberToTeamAsync_WhenSuccessful_CompletesWithoutException()
    {
        // Arrange
        var response = new AddMemberToTeamResponse();
        _mockClient.Setup(c => c.AddMemberToTeamAsync(
            It.IsAny<AddMemberToTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act & Assert - Should not throw
        await _service.AddMemberToTeamAsync("team-123", "user-456", 1);
    }

    [Fact]
    public async Task RemoveMemberFromTeamAsync_WhenSuccessful_CompletesWithoutException()
    {
        // Arrange
        var response = new RemoveMemberFromTeamResponse();
        _mockClient.Setup(c => c.RemoveMemberFromTeamAsync(
            It.IsAny<RemoveMemberFromTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act & Assert - Should not throw
        await _service.RemoveMemberFromTeamAsync("team-123", "user-456");
    }

    #endregion
}
