namespace our_group.Shared.Domain
{
    public class UserName
    {
        public string Value { get; }
        public UserName(string value)
        {
            Value = value;
        }
        public override string ToString() => Value;
    }
}

