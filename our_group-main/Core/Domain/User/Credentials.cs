namespace our_group.Core.Domain.User
{
    public class Credentials
    {
        public string Username { get; }
        public string Password { get; }
        public Credentials(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
