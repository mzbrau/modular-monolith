using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TicketSystem.User.Domain;

namespace TicketSystem.User.Infrastructure.Grpc;

internal class UserGrpcService : UserService.UserServiceBase
{
    private readonly Application.UserService _userService;
    private readonly ILogger<UserGrpcService> _logger;

    public UserGrpcService(Application.UserService userService, ILogger<UserGrpcService> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC CreateUser called for email: {Email}", request.Email);
        try
        {
            var userId = await _userService.CreateUserAsync(request.Email, request.FirstName, request.LastName);
            _logger.LogInformation("gRPC CreateUser succeeded: {UserId}", userId);
            return new CreateUserResponse { UserId = userId.ToString() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC CreateUser failed for email: {Email}", request.Email);
            throw;
        }
    }

    public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        _logger.LogDebug("gRPC GetUser called for user ID: {UserId}", request.UserId);
        
        if (!long.TryParse(request.UserId, out var userId))
        {
            _logger.LogWarning("gRPC GetUser - Invalid user ID format: {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user_id"));
        }

        try
        {
            var user = await _userService.GetUserAsync(userId);
            return new GetUserResponse { User = MapToMessage(user) };
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("gRPC GetUser - User not found: {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found"));
        }
    }

    public override async Task<ListUsersResponse> ListUsers(ListUsersRequest request, ServerCallContext context)
    {
        var users = await _userService.GetAllUsersAsync();
        var response = new ListUsersResponse();
        response.Users.AddRange(users.Where(u => u != null).Select(MapToMessage));
        return response;
    }

    public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        try
        {
            var userId = long.Parse(request.UserId);
            await _userService.UpdateUserAsync(userId, request.FirstName, request.LastName);
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
            await _userService.DeactivateUserAsync(userId);
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
        var user = await _userService.FindUserByEmailAsync(request.Email);
        return new FindUserByEmailResponse { User = user != null ? MapToMessage(user) : null };
    }

    private static UserMessage MapToMessage(UserBusinessEntity user)
    {
        return new UserMessage
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            DisplayName = user.DisplayName ?? string.Empty,
            CreatedDate = user.CreatedDate.ToString("O"),
            IsActive = user.IsActive
        };
    }
}
