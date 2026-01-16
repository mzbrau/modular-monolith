namespace TicketSystem.Team.Domain;

public readonly struct TeamId : IEquatable<TeamId>
{
    public Guid Value { get; }

    public TeamId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TeamId cannot be empty.", nameof(value));
        Value = value;
    }

    public static TeamId New() => new(Guid.NewGuid());
    
    public static TeamId Parse(string value) => new(Guid.Parse(value));

    public bool Equals(TeamId other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is TeamId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public static bool operator ==(TeamId left, TeamId right) => left.Equals(right);

    public static bool operator !=(TeamId left, TeamId right) => !left.Equals(right);

    public static implicit operator Guid(TeamId teamId) => teamId.Value;
    
    public static implicit operator TeamId(Guid guid) => new(guid);
}
