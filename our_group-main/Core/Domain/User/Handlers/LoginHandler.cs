using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace our_group.Core.Domain.User
{
    public class LoginHandler : IRequestHandler<Login, LoginResult>
    {
        private readonly IUserRepository _repo;
        private readonly IMediator _mediator;

        public LoginHandler(IUserRepository repo, IMediator mediator)
        {
            _repo = repo;
            _mediator = mediator;
        }

        public async Task<LoginResult> Handle(Login request, CancellationToken cancellationToken)
        {
            var account = await _repo.AuthenticateAsync(request.Username, request.Password);
            if (account == null)
            {
                return new LoginResult(null, null);
            }

            await _mediator.Publish(new UserLoggedIn(account), cancellationToken);
            return new LoginResult(account, null);
        }
    }
}
