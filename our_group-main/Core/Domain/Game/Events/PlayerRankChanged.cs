using MediatR;

namespace our_group.Core.Domain.Game.Events
{
    public record PlayerRankChanged(int PlayerId, int OldRank, int NewRank) : INotification;
}

