namespace our_group.Shared.Domain
{
    public class EmailAddress
    {
        public string Value { get; }
        public EmailAddress(string value)
        {
            Value = value;
        }
        public override string ToString() => Value;
    }
}

