using MediatR;

namespace our_group.Core.Domain.User
{
    public record Login(string Username, string Password) : IRequest<LoginResult>;

    public record LoginResult(UserAccount? Account, string? Jwt);
}

