using Grpc.Core;
using Grpc.Core.Testing;
using Moq;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.User.Infrastructure.Grpc;
using UserModel = TicketSystem.Client.Wpf.Models.User;

namespace TicketSystem.Client.Wpf.Tests.Services;

/// <summary>
/// Unit tests for UserService.
/// Tests model transformation from UserMessage to User and error handling scenarios.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<TicketSystem.User.Infrastructure.Grpc.UserService.UserServiceClient> _mockClient;
    private readonly TicketSystem.Client.Wpf.Services.UserService _service;

    public UserServiceTests()
    {
        _mockClient = new Mock<TicketSystem.User.Infrastructure.Grpc.UserService.UserServiceClient>();
        _service = new TicketSystem.Client.Wpf.Services.UserService(_mockClient.Object);
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
    public async Task GetAllUsersAsync_TransformsUserMessageToUser_Correctly()
    {
        // Arrange
        var userMessage = new UserMessage
        {
            Id = "user-123",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "John Doe",
            CreatedDate = "2024-01-15T10:30:00Z",
            IsActive = true
        };

        var response = new ListUsersResponse();
        response.Users.Add(userMessage);

        _mockClient.Setup(c => c.ListUsersAsync(
            It.IsAny<ListUsersRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.GetAllUsersAsync();
        var user = result.First();

        // Assert
        Assert.Equal("user-123", user.Id);
        Assert.Equal("john.doe@example.com", user.Email);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("John Doe", user.DisplayName);
        Assert.NotEqual(DateTime.MinValue, user.CreatedDate);
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task GetUserAsync_TransformsInactiveUser_Correctly()
    {
        // Arrange
        var userMessage = new UserMessage
        {
            Id = "user-123",
            Email = "inactive@example.com",
            FirstName = "Inactive",
            LastName = "User",
            DisplayName = "Inactive User",
            CreatedDate = "2024-01-15T10:30:00Z",
            IsActive = false
        };

        var response = new GetUserResponse { User = userMessage };

        _mockClient.Setup(c => c.GetUserAsync(
            It.IsAny<GetUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.GetUserAsync("user-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user-123", result.Id);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task GetAllUsersAsync_TransformsMultipleUsers_Correctly()
    {
        // Arrange
        var response = new ListUsersResponse();
        response.Users.Add(new UserMessage
        {
            Id = "user-1",
            Email = "user1@example.com",
            FirstName = "User",
            LastName = "One",
            DisplayName = "User One",
            CreatedDate = "2024-01-15T10:30:00Z",
            IsActive = true
        });
        response.Users.Add(new UserMessage
        {
            Id = "user-2",
            Email = "user2@example.com",
            FirstName = "User",
            LastName = "Two",
            DisplayName = "User Two",
            CreatedDate = "2024-01-16T10:30:00Z",
            IsActive = false
        });

        _mockClient.Setup(c => c.ListUsersAsync(
            It.IsAny<ListUsersRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.GetAllUsersAsync();
        var users = result.ToList();

        // Assert
        Assert.Equal(2, users.Count);
        Assert.Equal("user-1", users[0].Id);
        Assert.Equal("user-2", users[1].Id);
        Assert.True(users[0].IsActive);
        Assert.False(users[1].IsActive);
    }

    [Fact]
    public async Task FindUserByEmailAsync_TransformsUserMessage_Correctly()
    {
        // Arrange
        var userMessage = new UserMessage
        {
            Id = "user-123",
            Email = "found@example.com",
            FirstName = "Found",
            LastName = "User",
            DisplayName = "Found User",
            CreatedDate = "2024-01-15T10:30:00Z",
            IsActive = true
        };

        var response = new FindUserByEmailResponse { User = userMessage };

        _mockClient.Setup(c => c.FindUserByEmailAsync(
            It.IsAny<FindUserByEmailRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.FindUserByEmailAsync("found@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user-123", result.Id);
        Assert.Equal("found@example.com", result.Email);
    }

    #endregion

    #region Network Error Tests

    [Fact]
    public async Task GetAllUsersAsync_WhenUnavailable_ThrowsInvalidOperationExceptionWithNetworkMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Unavailable, "Service unavailable"));
        _mockClient.Setup(c => c.ListUsersAsync(
            It.IsAny<ListUsersRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<ListUsersResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetAllUsersAsync());
        Assert.Contains("Unable to connect to the server", exception.Message);
    }

    [Fact]
    public async Task CreateUserAsync_WhenDeadlineExceeded_ThrowsInvalidOperationExceptionWithNetworkMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline exceeded"));
        _mockClient.Setup(c => c.CreateUserAsync(
            It.IsAny<CreateUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<CreateUserResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateUserAsync("test@example.com", "Test", "User"));
        Assert.Contains("Unable to connect to the server", exception.Message);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenCancelled_ThrowsInvalidOperationExceptionWithNetworkMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Cancelled, "Request cancelled"));
        _mockClient.Setup(c => c.UpdateUserAsync(
            It.IsAny<UpdateUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<UpdateUserResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateUserAsync("user-123", "Updated", "Name"));
        Assert.Contains("Unable to connect to the server", exception.Message);
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task CreateUserAsync_WhenInvalidArgument_ThrowsInvalidOperationExceptionWithValidationMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.InvalidArgument, "Email is required"));
        _mockClient.Setup(c => c.CreateUserAsync(
            It.IsAny<CreateUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<CreateUserResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateUserAsync("", "Test", "User"));
        Assert.Contains("Validation failed", exception.Message);
        Assert.Contains("Email is required", exception.Message);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenFailedPrecondition_ThrowsInvalidOperationExceptionWithValidationMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.FailedPrecondition, "User is deactivated"));
        _mockClient.Setup(c => c.UpdateUserAsync(
            It.IsAny<UpdateUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<UpdateUserResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateUserAsync("user-123", "Test", "User"));
        Assert.Contains("Validation failed", exception.Message);
        Assert.Contains("User is deactivated", exception.Message);
    }

    #endregion

    #region NotFound Error Tests

    [Fact]
    public async Task GetUserAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "User not found"));
        _mockClient.Setup(c => c.GetUserAsync(
            It.IsAny<GetUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<GetUserResponse>(rpcException));

        // Act
        var result = await _service.GetUserAsync("nonexistent-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindUserByEmailAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "User not found"));
        _mockClient.Setup(c => c.FindUserByEmailAsync(
            It.IsAny<FindUserByEmailRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<FindUserByEmailResponse>(rpcException));

        // Act
        var result = await _service.FindUserByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenNotFound_ThrowsInvalidOperationExceptionWithNotFoundMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "User not found"));
        _mockClient.Setup(c => c.UpdateUserAsync(
            It.IsAny<UpdateUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<UpdateUserResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateUserAsync("nonexistent-id", "Test", "User"));
        Assert.Contains("User not found", exception.Message);
    }

    [Fact]
    public async Task DeactivateUserAsync_WhenNotFound_ThrowsInvalidOperationExceptionWithNotFoundMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.NotFound, "User not found"));
        _mockClient.Setup(c => c.DeactivateUserAsync(
            It.IsAny<DeactivateUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<DeactivateUserResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeactivateUserAsync("nonexistent-id"));
        Assert.Contains("User not found", exception.Message);
    }

    #endregion

    #region Unexpected Error Tests

    [Fact]
    public async Task GetAllUsersAsync_WhenUnexpectedError_ThrowsInvalidOperationExceptionWithGenericMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        _mockClient.Setup(c => c.ListUsersAsync(
            It.IsAny<ListUsersRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<ListUsersResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetAllUsersAsync());
        Assert.Contains("Failed to retrieve users", exception.Message);
    }

    [Fact]
    public async Task CreateUserAsync_WhenUnexpectedError_ThrowsInvalidOperationExceptionWithGenericMessage()
    {
        // Arrange
        var rpcException = new RpcException(new Status(StatusCode.Unknown, "Unknown error"));
        _mockClient.Setup(c => c.CreateUserAsync(
            It.IsAny<CreateUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCallWithException<CreateUserResponse>(rpcException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateUserAsync("test@example.com", "Test", "User"));
        Assert.Contains("Failed to create user", exception.Message);
    }

    #endregion

    #region Successful Operation Tests

    [Fact]
    public async Task CreateUserAsync_WhenSuccessful_ReturnsUserId()
    {
        // Arrange
        var response = new CreateUserResponse { UserId = "new-user-123" };
        _mockClient.Setup(c => c.CreateUserAsync(
            It.IsAny<CreateUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act
        var result = await _service.CreateUserAsync("john.doe@example.com", "John", "Doe");

        // Assert
        Assert.Equal("new-user-123", result);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenSuccessful_CompletesWithoutException()
    {
        // Arrange
        var response = new UpdateUserResponse();
        _mockClient.Setup(c => c.UpdateUserAsync(
            It.IsAny<UpdateUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act & Assert - Should not throw
        await _service.UpdateUserAsync("user-123", "Updated", "Name");
    }

    [Fact]
    public async Task DeactivateUserAsync_WhenSuccessful_CompletesWithoutException()
    {
        // Arrange
        var response = new DeactivateUserResponse();
        _mockClient.Setup(c => c.DeactivateUserAsync(
            It.IsAny<DeactivateUserRequest>(),
            null,
            null,
            default))
            .Returns(CreateAsyncUnaryCall(response));

        // Act & Assert - Should not throw
        await _service.DeactivateUserAsync("user-123");
    }

    #endregion
}
