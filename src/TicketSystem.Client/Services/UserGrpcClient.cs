using Grpc.Net.Client;
using TicketSystem.User.Grpc;

namespace TicketSystem.Client.Services;

public class UserGrpcClient : IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly UserService.UserServiceClient _client;

    public UserGrpcClient(string grpcAddress)
    {
        _channel = GrpcChannel.ForAddress(grpcAddress);
        _client = new UserService.UserServiceClient(_channel);
    }

    public async Task<string> CreateUserAsync(string email, string firstName, string lastName)
    {
        var response = await _client.CreateUserAsync(new CreateUserRequest
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName
        });
        return response.UserId;
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
        var response = await _client.ListUsersAsync(new ListUsersRequest());
        return response.Users.ToList();
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
