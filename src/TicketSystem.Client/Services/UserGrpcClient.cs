using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using TicketSystem.User.Infrastructure.Grpc;

namespace TicketSystem.Client.Services;

public class UserGrpcClient : IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly UserService.UserServiceClient _client;
    private readonly ILogger<UserGrpcClient> _logger;

    public UserGrpcClient(string grpcAddress, ILogger<UserGrpcClient> logger)
    {
        _channel = GrpcChannel.ForAddress(grpcAddress);
        _client = new UserService.UserServiceClient(_channel);
        _logger = logger;
        _logger.LogInformation("UserGrpcClient initialized with address: {GrpcAddress}", grpcAddress);
    }

    public async Task<string> CreateUserAsync(string email, string firstName, string lastName)
    {
        _logger.LogInformation("Creating user via gRPC: {Email}", email);
        try
        {
            var response = await _client.CreateUserAsync(new CreateUserRequest
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName
            });
            _logger.LogInformation("User created successfully via gRPC: {UserId}", response.UserId);
            return response.UserId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user via gRPC: {Email}", email);
            throw;
        }
    }

    public async Task<UserMessage?> GetUserAsync(string userId)
    {
        try
        {
            var response = await _client.GetUserAsync(new GetUserRequest { UserId = userId });
            return response.User;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<UserMessage>> ListUsersAsync()
    {
        _logger.LogDebug("Fetching user list via gRPC");
        try
        {
            var response = await _client.ListUsersAsync(new ListUsersRequest());
            _logger.LogDebug("Retrieved {Count} users via gRPC", response.Users.Count);
            return response.Users.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user list via gRPC");
            throw;
        }
    }

    public async Task UpdateUserAsync(string userId, string firstName, string lastName)
    {
        await _client.UpdateUserAsync(new UpdateUserRequest
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName
        });
    }

    public async Task DeactivateUserAsync(string userId)
    {
        await _client.DeactivateUserAsync(new DeactivateUserRequest { UserId = userId });
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}
