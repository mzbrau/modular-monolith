using TicketSystem.User.Contracts;
using TicketSystem.User.Domain;

namespace TicketSystem.User.Application.Adapters;

internal static class UserConverter
{
    public static UserDataContract ToDataContract(UserBusinessEntity user)
    {
        return new UserDataContract
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            CreatedDate = user.CreatedDate,
            IsActive = user.IsActive
        };
    }
}
