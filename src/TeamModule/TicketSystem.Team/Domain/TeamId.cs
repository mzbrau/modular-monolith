namespace TicketSystem.Team.Domain;

public readonly struct TeamId : IEquatable<TeamId>
{
    public long Value { get; }

    public TeamId(long value)
    {
        if (value <= 0)
            throw new ArgumentException("TeamId must be greater than zero.", nameof(value));
        Value = value;
    }

    public static TeamId Parse(string value) => new(long.Parse(value));

    public bool Equals(TeamId other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is TeamId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public static bool operator ==(TeamId left, TeamId right) => left.Equals(right);

    public static bool operator !=(TeamId left, TeamId right) => !left.Equals(right);

    public static implicit operator long(TeamId teamId) => teamId.Value;
    
    public static implicit operator TeamId(long id) => new(id);
}
