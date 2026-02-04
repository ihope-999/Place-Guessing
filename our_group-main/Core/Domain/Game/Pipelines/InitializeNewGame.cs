using System;
using System.Threading;
using System.Threading.Tasks;
using our_group.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace our_group.Core.Domain.Game.Pipelines;

public class InitializeNewGame{
    public record Request(Game NewGame) : IRequest<Game?>;

    public class Handler : IRequestHandler<Request, Game?>{
        private readonly GameContext _db;

        public Handler(GameContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<Game?> Handle(Request request, CancellationToken cancellationToken){
            _db.Games.Add(request.NewGame);
            await _db.SaveChangesAsync(cancellationToken);

            return request.NewGame;
        }
    }
}