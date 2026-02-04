using MediatR;

namespace our_group.Core.Domain.User
{
    public record UserRegistered(UserAccount Account) : INotification;
}

