using MediatR;

namespace our_group.Core.Domain.User
{
    public record UserProfileUpdated(UserAccount Account) : INotification;
}

