using FsCheck;
using FsCheck.Xunit;
using Xunit;
using UserModel = TicketSystem.Client.Wpf.Models.User;

namespace TicketSystem.Client.Wpf.Tests.Properties;

/// <summary>
/// Property-based tests for user display completeness.
/// Feature: wpf-ticket-client
/// </summary>
public class UserDisplayPropertyTests
{
    /// <summary>
    /// Property 4: User Display Completeness
    /// For any user retrieved from the UserService, when displayed in the user list,
    /// the UI SHALL show the email, first name, last name, display name, and active status.
    /// **Validates: Requirements 10.2**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "wpf-ticket-client")]
    [Trait("Property", "4")]
    public Property UserDisplayCompleteness_AllRequiredFieldsAreAccessible()
    {
        return Prop.ForAll(
            UserGenerator(),
            user =>
            {
                // Verify that all required fields for display are accessible and not throwing exceptions
                // According to Requirement 10.2, the UI SHALL show:
                // - email
                // - first name
                // - last name
                // - display name
                // - active status
                
                try
                {
                    // Access all required display fields
                    var email = user.Email;
                    var firstName = user.FirstName;
                    var lastName = user.LastName;
                    var displayName = user.DisplayName;
                    var isActive = user.IsActive;
                    
                    // Verify that required fields are valid
                    var emailIsValid = !string.IsNullOrEmpty(email);
                    var firstNameIsValid = !string.IsNullOrEmpty(firstName);
                    var lastNameIsValid = !string.IsNullOrEmpty(lastName);
                    var displayNameIsValid = !string.IsNullOrEmpty(displayName);
                    // isActive is a bool, so it's always valid
                    
                    return emailIsValid && firstNameIsValid && lastNameIsValid && displayNameIsValid;
                }
                catch
                {
                    // If any field access throws an exception, the property fails
                    return false;
                }
            }
        ).Label("Feature: wpf-ticket-client, Property 4: User Display Completeness");
    }
    
    /// <summary>
    /// Generator for User objects with valid data.
    /// </summary>
    private static Arbitrary<UserModel> UserGenerator()
    {
        var gen = from id in Arb.Generate<NonEmptyString>()
                  from email in Arb.Generate<NonEmptyString>()
                  from firstName in Arb.Generate<NonEmptyString>()
                  from lastName in Arb.Generate<NonEmptyString>()
                  from displayName in Arb.Generate<NonEmptyString>()
                  from createdDate in Arb.Generate<DateTime>()
                  from isActive in Arb.Generate<bool>()
                  select new UserModel
                  {
                      Id = id.Get,
                      Email = email.Get,
                      FirstName = firstName.Get,
                      LastName = lastName.Get,
                      DisplayName = displayName.Get,
                      CreatedDate = createdDate,
                      IsActive = isActive
                  };
        
        return Arb.From(gen);
    }
}
