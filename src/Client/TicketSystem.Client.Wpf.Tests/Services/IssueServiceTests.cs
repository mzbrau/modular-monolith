using Grpc.Core;
using Grpc.Core.Testing;
using Moq;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Issue.Infrastructure.Grpc;
using IssueModel = TicketSystem.Client.Wpf.Models.Issue;

namespace TicketSystem.Client.Wpf.Tests.Services;

/// <summary>
/// Unit tests for IssueService.
/// Tests model transformation from IssueMessage to Issue and error handling scenarios.
/// </summary>
public class IssueServiceTests
{
    private readonly Mock<TicketSystem.Issue.Infrastructure.Grpc.IssueService.IssueServiceClient> _mockClient;
    private readonly TicketSystem.Client.Wpf.Services.IssueService _service;

    public IssueServiceTests()
    {
        _mockClient = new Mock<TicketSystem.Issue.Infrastructure.Grpc.IssueService.IssueServiceClient>();
        _service = new TicketSystem.Client.Wpf.Services.IssueService(_mockClient.Object);
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
    public async Task GetAllIssuesAsync_TransformsIssueMessageToIssue_Correctly()
    {
        // Arrange
        var issueMessage = new IssueMessage
        {
            Id = "issue-123",
            Title = "Test Issue",
            Description = "Test Description",
            Status = 1, // InProgress
            Priority = 5,
            AssignedUserId = "user-456",
            AssignedTeamId = "team-789",
            CreatedDate = "2024-01-15T10:30:00Z",
            DueDate = "2024-02-15T10:30:00Z",
            ResolvedDate = "",
            LastModifiedDate = "2024-01-16T14:20:00Z"
        };

        var response = new ListIssuesResponse();
        response.Issues.Add(issueMessage);

        _mockClient.Setup(c => c.ListIssuesAsync(
            It.IsAny<ListIssuesRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.GetAllIssuesAsync();
        var issue = result.First();

        // Assert
        Assert.Equal("issue-123", issue.Id);
        Assert.Equal("Test Issue", issue.Title);
        Assert.Equal("Test Description", issue.Description);
        Assert.Equal(IssueStatus.InProgress, issue.Status);
        Assert.Equal(5, issue.Priority);
        Assert.Equal("user-456", issue.AssignedUserId);
        Assert.Equal("team-789", issue.AssignedTeamId);
        Assert.NotEqual(DateTime.MinValue, issue.CreatedDate);
        Assert.NotNull(issue.DueDate);
        Assert.Null(issue.ResolvedDate);
        Assert.NotEqual(DateTime.MinValue, issue.LastModifiedDate);
    }

    [Fact]
    public async Task GetIssueAsync_TransformsIssueMessageWithNullableFields_Correctly()
    {
        // Arrange
        var issueMessage = new IssueMessage
        {
            Id = "issue-123",
            Title = "Test Issue",
            Description = "Test Description",
            Status = 0, // Open
            Priority = 3,
            AssignedUserId = "",
            AssignedTeamId = "",
            CreatedDate = "2024-01-15T10:30:00Z",
            DueDate = "",
            ResolvedDate = "",
            LastModifiedDate = "2024-01-15T10:30:00Z"
        };

        var response = new GetIssueResponse { Issue = issueMessage };

        _mockClient.Setup(c => c.GetIssueAsync(
            It.IsAny<GetIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.GetIssueAsync("issue-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("issue-123", result.Id);
        Assert.Null(result.AssignedUserId);
        Assert.Null(result.AssignedTeamId);
        Assert.Null(result.DueDate);
        Assert.Null(result.ResolvedDate);
    }

    [Fact]
    public async Task GetAllIssuesAsync_TransformsMultipleIssues_Correctly()
    {
        // Arrange
        var response = new ListIssuesResponse();
        response.Issues.Add(new IssueMessage
        {
            Id = "issue-1",
            Title = "Issue 1",
            Description = "Description 1",
            Status = 0,
            Priority = 1,
            CreatedDate = "2024-01-15T10:30:00Z",
            LastModifiedDate = "2024-01-15T10:30:00Z"
        });
        response.Issues.Add(new IssueMessage
        {
            Id = "issue-2",
            Title = "Issue 2",
            Description = "Description 2",
            Status = 3,
            Priority = 5,
            CreatedDate = "2024-01-16T10:30:00Z",
            LastModifiedDate = "2024-01-16T10:30:00Z"
        });

        _mockClient.Setup(c => c.ListIssuesAsync(
            It.IsAny<ListIssuesRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.GetAllIssuesAsync();
        var issues = result.ToList();

        // Assert
        Assert.Equal(2, issues.Count);
        Assert.Equal("issue-1", issues[0].Id);
        Assert.Equal("issue-2", issues[1].Id);
        Assert.Equal(IssueStatus.Open, issues[0].Status);
        Assert.Equal(IssueStatus.Resolved, issues[1].Status);
    }

    #endregion

    #region Network Error Tests

    [Fact]
    public async Task GetAllIssuesAsync_WhenUnavailable_ThrowsInvalidOperationExceptionWithNetworkMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Unavailable, "Service unavailable"));
        _mockClient.Setup(c => c.ListIssuesAsync(
            It.IsAny<ListIssuesRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<ListIssuesResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetAllIssuesAsync());
        Assert.Contains("Unable to connect to the server", exception.Message);
    }

    [Fact]
    public async Task CreateIssueAsync_WhenDeadlineExceeded_ThrowsInvalidOperationExceptionWithNetworkMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline exceeded"));
        _mockClient.Setup(c => c.CreateIssueAsync(
            It.IsAny<CreateIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<CreateIssueResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateIssueAsync("Test", "Description", 1, null));
        Assert.Contains("Unable to connect to the server", exception.Message);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenCancelled_ThrowsInvalidOperationExceptionWithNetworkMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Cancelled, "Request cancelled"));
        _mockClient.Setup(c => c.UpdateIssueAsync(
            It.IsAny<UpdateIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<UpdateIssueResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateIssueAsync("issue-123", "Test", "Description", 1, null));
        Assert.Contains("Unable to connect to the server", exception.Message);
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task CreateIssueAsync_WhenInvalidArgument_ThrowsInvalidOperationExceptionWithValidationMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.InvalidArgument, "Title is required"));
        _mockClient.Setup(c => c.CreateIssueAsync(
            It.IsAny<CreateIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<CreateIssueResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateIssueAsync("", "Description", 1, null));
        Assert.Contains("Validation failed", exception.Message);
        Assert.Contains("Title is required", exception.Message);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenFailedPrecondition_ThrowsInvalidOperationExceptionWithValidationMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.FailedPrecondition, "Issue is closed"));
        _mockClient.Setup(c => c.UpdateIssueAsync(
            It.IsAny<UpdateIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<UpdateIssueResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateIssueAsync("issue-123", "Test", "Description", 1, null));
        Assert.Contains("Validation failed", exception.Message);
        Assert.Contains("Issue is closed", exception.Message);
    }

    [Fact]
    public async Task UpdateIssueStatusAsync_WhenOutOfRange_ThrowsInvalidOperationExceptionWithValidationMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.OutOfRange, "Invalid status value"));
        _mockClient.Setup(c => c.UpdateIssueStatusAsync(
            It.IsAny<UpdateIssueStatusRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<UpdateIssueStatusResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateIssueStatusAsync("issue-123", IssueStatus.Open));
        Assert.Contains("Validation failed", exception.Message);
        Assert.Contains("Invalid status value", exception.Message);
    }

    #endregion

    #region NotFound Error Tests

    [Fact]
    public async Task GetIssueAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "Issue not found"));
        _mockClient.Setup(c => c.GetIssueAsync(
            It.IsAny<GetIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<GetIssueResponse>(rpcException));

        // Act
        var result = await _service.GetIssueAsync("nonexistent-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenNotFound_ThrowsInvalidOperationExceptionWithNotFoundMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "Issue not found"));
        _mockClient.Setup(c => c.UpdateIssueAsync(
            It.IsAny<UpdateIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<UpdateIssueResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateIssueAsync("nonexistent-id", "Test", "Description", 1, null));
        Assert.Contains("Issue not found", exception.Message);
    }

    [Fact]
    public async Task DeleteIssueAsync_WhenNotFound_ThrowsInvalidOperationExceptionWithNotFoundMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "Issue not found"));
        _mockClient.Setup(c => c.DeleteIssueAsync(
            It.IsAny<DeleteIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<DeleteIssueResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteIssueAsync("nonexistent-id"));
        Assert.Contains("Issue not found", exception.Message);
    }

    #endregion

    #region Unexpected Error Tests

    [Fact]
    public async Task GetAllIssuesAsync_WhenUnexpectedError_ThrowsInvalidOperationExceptionWithGenericMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        _mockClient.Setup(c => c.ListIssuesAsync(
            It.IsAny<ListIssuesRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<ListIssuesResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetAllIssuesAsync());
        Assert.Contains("Failed to retrieve issues", exception.Message);
    }

    [Fact]
    public async Task CreateIssueAsync_WhenUnexpectedError_ThrowsInvalidOperationExceptionWithGenericMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Unknown, "Unknown error"));
        _mockClient.Setup(c => c.CreateIssueAsync(
            It.IsAny<CreateIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<CreateIssueResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateIssueAsync("Test", "Description", 1, null));
        Assert.Contains("Failed to create issue", exception.Message);
    }

    #endregion

    #region Successful Operation Tests

    [Fact]
    public async Task CreateIssueAsync_WhenSuccessful_ReturnsIssueId()
    {
        // Arrange
        var response = new CreateIssueResponse { IssueId = "new-issue-123" };
        _mockClient.Setup(c => c.CreateIssueAsync(
            It.IsAny<CreateIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.CreateIssueAsync("Test Issue", "Test Description", 3, DateTime.Now.AddDays(7));

        // Assert
        Assert.Equal("new-issue-123", result);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenSuccessful_CompletesWithoutException()
    {
        // Arrange
        var response = new UpdateIssueResponse();
        _mockClient.Setup(c => c.UpdateIssueAsync(
            It.IsAny<UpdateIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act & Assert - Should not throw
        await _service.UpdateIssueAsync("issue-123", "Updated Title", "Updated Description", 5, null);
    }

    [Fact]
    public async Task DeleteIssueAsync_WhenSuccessful_CompletesWithoutException()
    {
        // Arrange
        var response = new DeleteIssueResponse();
        _mockClient.Setup(c => c.DeleteIssueAsync(
            It.IsAny<DeleteIssueRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act & Assert - Should not throw
        await _service.DeleteIssueAsync("issue-123");
    }

    [Fact]
    public async Task AssignIssueToUserAsync_WhenSuccessful_CompletesWithoutException()
    {
        // Arrange
        var response = new AssignIssueToUserResponse();
        _mockClient.Setup(c => c.AssignIssueToUserAsync(
            It.IsAny<AssignIssueToUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act & Assert - Should not throw
        await _service.AssignIssueToUserAsync("issue-123", "user-456");
    }

    [Fact]
    public async Task AssignIssueToTeamAsync_WhenSuccessful_CompletesWithoutException()
    {
        // Arrange
        var response = new AssignIssueToTeamResponse();
        _mockClient.Setup(c => c.AssignIssueToTeamAsync(
            It.IsAny<AssignIssueToTeamRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act & Assert - Should not throw
        await _service.AssignIssueToTeamAsync("issue-123", "team-789");
    }

    #endregion
}
