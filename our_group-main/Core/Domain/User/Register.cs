using MediatR;

namespace our_group.Core.Domain.User
{
    public record Register(string Username, string Email, string Password) : IRequest<RegisterResult>;

    public record RegisterResult(UserAccount Account);
}

