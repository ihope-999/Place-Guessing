using MediatR;

namespace our_group.Core.Domain.User
{
    public record UserLoggedIn(UserAccount Account) : INotification;
}

