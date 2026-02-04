using MediatR;

namespace our_group.Core.Domain.User
{
    public record UserRatingChanged(string UserId, int OldRating, int NewRating) : INotification;
}

