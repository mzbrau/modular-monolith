using System;

namespace TicketSystem.User.Domain;

public class UserBusinessEntity
{
    public virtual long Id { get; protected set; }
    public virtual string Email { get; protected set; } = string.Empty;
    public virtual string FirstName { get; protected set; } = string.Empty;
    public virtual string LastName { get; protected set; } = string.Empty;
    public virtual DateTime CreatedDate { get; protected set; }
    public virtual bool IsActive { get; protected set; }

    public virtual string DisplayName => $"{FirstName} {LastName}";

    protected UserBusinessEntity() { }

    public UserBusinessEntity(long id, string email, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName cannot be empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName cannot be empty.", nameof(lastName));

        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        CreatedDate = DateTime.UtcNow;
        IsActive = true;
    }

    public virtual void Update(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName cannot be empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName cannot be empty.", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
    }

    public virtual void Deactivate()
    {
        IsActive = false;
    }

    public virtual void Activate()
    {
        IsActive = true;
    }
}
