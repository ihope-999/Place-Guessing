using System;
using System.Threading;
using System.Threading.Tasks;
using our_group.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace our_group.Core.Domain.Game.Pipelines;

public class StoreGame
{
    public record Request(Game Game) : IRequest<Unit>;

    public class Handler : IRequestHandler<Request, Unit>
    {
        private readonly GameContext _db;

        public Handler(GameContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            _db.Games.Update(request.Game);
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

    }
}