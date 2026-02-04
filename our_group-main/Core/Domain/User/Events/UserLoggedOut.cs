using MediatR;

namespace our_group.Core.Domain.User
{
    public record UserLoggedOut(UserAccount Account) : INotification;
}

