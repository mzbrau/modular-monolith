using Grpc.Core;
using TicketSystem.Api.Protos;
using TicketSystem.User.Contracts;

namespace TicketSystem.Api.Services;

public class UserGrpcService : UserService.UserServiceBase
{
    private readonly IUserModuleApi _userModuleApi;

    public UserGrpcService(IUserModuleApi userModuleApi)
    {
        _userModuleApi = userModuleApi;
    }

    public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var userId = await _userModuleApi.CreateUserAsync(request.Email, request.FirstName, request.LastName);
        return new CreateUserResponse { UserId = userId.ToString() };
    }

    public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        if (!long.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user_id"));

        var user = await _userModuleApi.GetUserAsync(userId);
        if (user == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found"));

        return new GetUserResponse { User = MapToMessage(user) };
    }

    public override async Task<ListUsersResponse> ListUsers(ListUsersRequest request, ServerCallContext context)
    {
        var users = await _userModuleApi.GetAllUsersAsync();
        var response = new ListUsersResponse();
        response.Users.AddRange(users.Select(MapToMessage));
        return response;
    }

    public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        try
        {
            var userId = long.Parse(request.UserId);
            await _userModuleApi.UpdateUserAsync(userId, request.FirstName, request.LastName);
            return new UpdateUserResponse();
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found"));
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user_id"));
        }
    }

    public override async Task<DeactivateUserResponse> DeactivateUser(DeactivateUserRequest request, ServerCallContext context)
    {
        try
        {
            var userId = long.Parse(request.UserId);
            await _userModuleApi.DeactivateUserAsync(userId);
            return new DeactivateUserResponse();
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found"));
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user_id"));
        }
    }

    public override async Task<FindUserByEmailResponse> FindUserByEmail(FindUserByEmailRequest request, ServerCallContext context)
    {
        var user = await _userModuleApi.FindUserByEmailAsync(request.Email);
        return new FindUserByEmailResponse { User = user != null ? MapToMessage(user) : null };
    }

    private static UserMessage MapToMessage(UserDataContract user)
    {
        return new UserMessage
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            CreatedDate = user.CreatedDate.ToString("O"),
            IsActive = user.IsActive
        };
    }
}
