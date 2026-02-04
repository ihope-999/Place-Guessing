using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace our_group.Core.Domain.User
{
    public class RegisterHandler : IRequestHandler<Register, RegisterResult>
    {
        private readonly IUserRepository _repo;
        private readonly IMediator _mediator;

        public RegisterHandler(IUserRepository repo, IMediator mediator)
        {
            _repo = repo;
            _mediator = mediator;
        }

        public async Task<RegisterResult> Handle(Register request, CancellationToken cancellationToken)
        {
            if (await _repo.ExistsByUsernameAsync(request.Username))
            {
                throw new System.InvalidOperationException("Username already exists");
            }
            if (await _repo.ExistsByEmailAsync(request.Email))
            {
                throw new System.InvalidOperationException("Email already exists");
            }

            var account = await _repo.CreateAsync(request.Username, request.Email, request.Password);
            await _mediator.Publish(new UserRegistered(account), cancellationToken);
            return new RegisterResult(account);
        }
    }
}

