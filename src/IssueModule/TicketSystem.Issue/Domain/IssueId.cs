namespace TicketSystem.Issue.Domain;

public readonly struct IssueId : IEquatable<IssueId>
{
    public Guid Value { get; }

    public IssueId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("IssueId cannot be empty.", nameof(value));
        Value = value;
    }

    public static IssueId New() => new(Guid.NewGuid());

    public static IssueId Parse(string value) => new(Guid.Parse(value));

    public bool Equals(IssueId other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is IssueId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public static bool operator ==(IssueId left, IssueId right) => left.Equals(right);

    public static bool operator !=(IssueId left, IssueId right) => !left.Equals(right);

    public static implicit operator Guid(IssueId issueId) => issueId.Value;

    public static implicit operator IssueId(Guid guid) => new(guid);
}
