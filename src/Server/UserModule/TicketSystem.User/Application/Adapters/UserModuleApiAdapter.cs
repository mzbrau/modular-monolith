using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSystem.User.Contracts;

namespace TicketSystem.User.Application.Adapters;

internal class UserModuleApiAdapter : IUserModuleApi
{
    private readonly UserService _userService;

    public UserModuleApiAdapter(UserService userService)
    {
        _userService = userService;
    }

    public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var userId = await _userService.CreateUserAsync(request.Email, request.FirstName, request.LastName);
        return new CreateUserResponse { UserId = userId };
    }

    public async Task<UserDataContract?> GetUserAsync(GetUserRequest request)
    {
        try
        {
            var user = await _userService.GetUserAsync(request.UserId);
            return UserConverter.ToDataContract(user);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<List<UserDataContract>> GetAllUsersAsync()
    {
        var users = await _userService.GetAllUsersAsync();
        return users.Select(UserConverter.ToDataContract).ToList();
    }

    public async Task<List<UserDataContract>> GetUsersAsync(GetUsersByIdsRequest request)
    {
        var users = await _userService.GetUsersByIdsAsync(request.UserIds);
        return users.Select(UserConverter.ToDataContract).ToList();
    }

    public async Task UpdateUserAsync(UpdateUserRequest request)
    {
        await _userService.UpdateUserAsync(request.UserId, request.FirstName, request.LastName);
    }

    public async Task DeactivateUserAsync(DeactivateUserRequest request)
    {
        await _userService.DeactivateUserAsync(request.UserId);
    }

    public async Task<UserDataContract?> FindUserByEmailAsync(FindUserByEmailRequest request)
    {
        var user = await _userService.FindUserByEmailAsync(request.Email);
        return user != null ? UserConverter.ToDataContract(user) : null;
    }

    public async Task<bool> UserExistsAsync(UserExistsRequest request)
    {
        return await _userService.UserExistsAsync(request.UserId);
    }
}
