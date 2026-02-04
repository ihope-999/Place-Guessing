namespace our_group.Shared.Domain
{
    public class UserId
    {
        public string Value { get; }
        public UserId(string value)
        {
            Value = value;
        }
        public override string ToString() => Value;
    }
}

