using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace our_group.Core.Domain.User
{
    public class UserRegisteredLogger : INotificationHandler<UserRegistered>
    {
        private readonly ILogger<UserRegisteredLogger> _logger;
        public UserRegisteredLogger(ILogger<UserRegisteredLogger> logger)
        {
            _logger = logger;
        }
        public Task Handle(UserRegistered notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("User registered: {User}", notification.Account.Name.Value);
            return Task.CompletedTask;
        }
    }

    public class UserLoggedInLogger : INotificationHandler<UserLoggedIn>
    {
        private readonly ILogger<UserLoggedInLogger> _logger;
        public UserLoggedInLogger(ILogger<UserLoggedInLogger> logger)
        {
            _logger = logger;
        }
        public Task Handle(UserLoggedIn notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("User logged in: {User}", notification.Account.Name.Value);
            return Task.CompletedTask;
        }
    }
}

