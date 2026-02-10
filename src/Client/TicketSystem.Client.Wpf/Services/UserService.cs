using Grpc.Core;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.User.Infrastructure.Grpc;

namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Service implementation for managing users in the ticket system.
/// Wraps the gRPC UserServiceClient and provides transformation between gRPC messages and domain models.
/// </summary>
public class UserService : IUserService
{
    private readonly TicketSystem.User.Infrastructure.Grpc.UserService.UserServiceClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="client">The gRPC client for user operations.</param>
    public UserService(TicketSystem.User.Infrastructure.Grpc.UserService.UserServiceClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Models.User>> GetAllUsersAsync()
    {
        try
        {
            var request = new ListUsersRequest();
            var response = await _client.ListUsersAsync(request);
            return response.Users.Select(MapToUser);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Invalid request: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve users: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Models.User?> GetUserAsync(string userId)
    {
        try
        {
            var request = new GetUserRequest { UserId = userId };
            var response = await _client.GetUserAsync(request);
            return response.User != null ? MapToUser(response.User) : null;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Invalid request: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve user: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<string> CreateUserAsync(string email, string firstName, string lastName)
    {
        try
        {
            var request = new CreateUserRequest
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };
            var response = await _client.CreateUserAsync(request);
            return response.UserId;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Validation failed: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to create user: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task UpdateUserAsync(string userId, string firstName, string lastName)
    {
        try
        {
            var request = new UpdateUserRequest
            {
                UserId = userId,
                FirstName = firstName,
                LastName = lastName
            };
            await _client.UpdateUserAsync(request);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Validation failed: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"User not found: {userId}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to update user: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeactivateUserAsync(string userId)
    {
        try
        {
            var request = new DeactivateUserRequest { UserId = userId };
            await _client.DeactivateUserAsync(request);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Validation failed: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidOperationException($"User not found: {userId}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deactivate user: {ex.Status.Detail}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Models.User?> FindUserByEmailAsync(string email)
    {
        try
        {
            var request = new FindUserByEmailRequest { Email = email };
            var response = await _client.FindUserByEmailAsync(request);
            return response.User != null ? MapToUser(response.User) : null;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                       ex.StatusCode == StatusCode.DeadlineExceeded ||
                                       ex.StatusCode == StatusCode.Cancelled)
        {
            throw new InvalidOperationException(
                "Unable to connect to the server. Please check your connection and try again.", ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument ||
                                       ex.StatusCode == StatusCode.FailedPrecondition ||
                                       ex.StatusCode == StatusCode.OutOfRange)
        {
            throw new InvalidOperationException($"Invalid request: {ex.Status.Detail}", ex);
        }
        catch (RpcException ex)
        {
            throw new InvalidOperationException(
                $"Failed to find user by email: {ex.Status.Detail}", ex);
        }
    }

    /// <summary>
    /// Maps a gRPC UserMessage to a domain User model.
    /// </summary>
    /// <param name="message">The gRPC message to map.</param>
    /// <returns>The mapped User domain model.</returns>
    private static Models.User MapToUser(UserMessage message)
    {
        return new Models.User
        {
            Id = message.Id,
            Email = message.Email,
            FirstName = message.FirstName,
            LastName = message.LastName,
            DisplayName = message.DisplayName,
            CreatedDate = ParseDateTime(message.CreatedDate),
            IsActive = message.IsActive
        };
    }

    /// <summary>
    /// Parses a date string to a DateTime object.
    /// </summary>
    /// <param name="dateString">The date string to parse.</param>
    /// <returns>The parsed DateTime.</returns>
    private static DateTime ParseDateTime(string dateString)
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return DateTime.MinValue;
        }

        if (DateTime.TryParse(dateString, out var result))
        {
            return result;
        }

        return DateTime.MinValue;
    }
}
