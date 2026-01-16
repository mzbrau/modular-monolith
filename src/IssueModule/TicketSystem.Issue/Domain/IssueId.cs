namespace TicketSystem.Issue.Domain;

public readonly struct IssueId : IEquatable<IssueId>
{
    public long Value { get; }

    public IssueId(long value)
    {
        if (value <= 0)
            throw new ArgumentException("IssueId must be greater than zero.", nameof(value));
        Value = value;
    }

    public static IssueId Parse(string value) => new(long.Parse(value));

    public bool Equals(IssueId other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is IssueId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public static bool operator ==(IssueId left, IssueId right) => left.Equals(right);

    public static bool operator !=(IssueId left, IssueId right) => !left.Equals(right);

    public static implicit operator long(IssueId issueId) => issueId.Value;

    public static implicit operator IssueId(long id) => new(id);
}
