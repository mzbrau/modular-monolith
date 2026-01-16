using TicketSystem.User.Domain;

namespace TicketSystem.User.Tests;

[TestFixture]
public class UserBusinessEntityTests
{
    [Test]
    public void Constructor_WithValidData_CreatesUser()
    {
        var userId = UserId.New();
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";

        var user = new UserBusinessEntity(userId, email, firstName, lastName);

        Assert.That(user.Id, Is.EqualTo(userId));
        Assert.That(user.Email, Is.EqualTo(email));
        Assert.That(user.FirstName, Is.EqualTo(firstName));
        Assert.That(user.LastName, Is.EqualTo(lastName));
        Assert.That(user.IsActive, Is.True);
        Assert.That(user.DisplayName, Is.EqualTo("John Doe"));
    }

    [Test]
    public void Constructor_WithEmptyEmail_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new UserBusinessEntity(UserId.New(), "", "John", "Doe"));
    }

    [Test]
    public void Constructor_WithEmptyFirstName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new UserBusinessEntity(UserId.New(), "test@example.com", "", "Doe"));
    }

    [Test]
    public void Update_WithValidData_UpdatesNameFields()
    {
        var user = new UserBusinessEntity(UserId.New(), "test@example.com", "John", "Doe");

        user.Update("Jane", "Smith");

        Assert.That(user.FirstName, Is.EqualTo("Jane"));
        Assert.That(user.LastName, Is.EqualTo("Smith"));
        Assert.That(user.DisplayName, Is.EqualTo("Jane Smith"));
    }

    [Test]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var user = new UserBusinessEntity(UserId.New(), "test@example.com", "John", "Doe");

        user.Deactivate();

        Assert.That(user.IsActive, Is.False);
    }

    [Test]
    public void Activate_SetsIsActiveToTrue()
    {
        var user = new UserBusinessEntity(UserId.New(), "test@example.com", "John", "Doe");
        user.Deactivate();

        user.Activate();

        Assert.That(user.IsActive, Is.True);
    }
}
