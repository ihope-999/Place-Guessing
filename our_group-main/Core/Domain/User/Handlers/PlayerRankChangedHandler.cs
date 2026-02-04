using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using our_group.Core.Domain.Game;
using our_group.Core.Domain.Game.Events;
using our_group.Infrastructure.Data;

namespace our_group.Core.Domain.User
{
    public class PlayerRankChangedHandler : INotificationHandler<PlayerRankChanged>
    {
        private readonly UserContext _db;
        private readonly ILogger<PlayerRankChangedHandler> _logger;

        public PlayerRankChangedHandler(UserContext db, ILogger<PlayerRankChangedHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Handle(PlayerRankChanged notification, CancellationToken cancellationToken)
        {
            var playerId = notification.PlayerId;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PlayerId == playerId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("PlayerRankChanged for unknown PlayerId {PlayerId}", playerId);
                return;
            }

            if (user.Rank != notification.NewRank)
            {
                var old = user.Rank;
                user.Rank = notification.NewRank;
                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Updated rank for {User} (PlayerId {PlayerId}) from {Old} to {New}", user.UserName, playerId, old, user.Rank);

            }
            else
            {
                _logger.LogDebug("Rank unchanged for {User} (PlayerId {PlayerId}) at {Rank}", user.UserName, playerId, user.Rank);
            }
        }
    }
}

