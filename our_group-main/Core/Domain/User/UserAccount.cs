using System;
using our_group.Shared.Domain;

namespace our_group.Core.Domain.User
{
    public class UserAccount
    {
        public UserId Id { get; }
        public UserName Name { get; }
        public EmailAddress Email { get; }
        public DateTime CreatedAt { get; }

        public UserAccount(UserId id, UserName name, EmailAddress email, DateTime createdAt)
        {
            Id = id;
            Name = name;
            Email = email;
            CreatedAt = createdAt;
        }
    }
}
