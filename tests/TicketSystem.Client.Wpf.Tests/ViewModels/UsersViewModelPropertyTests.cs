using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Moq;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Client.Wpf.ViewModels;
using UserModel = TicketSystem.Client.Wpf.Models.User;

namespace TicketSystem.Client.Wpf.Tests.ViewModels;

/// <summary>
/// Property-based tests for UsersViewModel class.
/// Feature: wpf-ticket-client
/// </summary>
public class UsersViewModelPropertyTests
{
    /// <summary>
    /// Property 7: Email Validation
    /// 
    /// **Validates: Requirements 11.6**
    /// 
    /// For any user creation operation, if the email is empty or not in valid email format,
    /// the validation SHALL fail and prevent the operation.
    /// 
    /// This property test verifies that:
    /// 1. Empty emails are rejected
    /// 2. Invalid email formats are rejected
    /// 3. Valid emails are accepted
    /// 4. Error message is set when validation fails
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property7_EmailValidation_CreateUser()
    {
        return Prop.ForAll(
            GenerateEmail(),
            email =>
            {
                // Arrange
                var mockUserService = new Mock<IUserService>();
                var mockDialogService = new Mock<IDialogService>();
                mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                var createCalled = false;
                mockUserService
                    .Setup(s => s.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Callback(() => createCalled = true)
                    .ReturnsAsync("new-user-id");

                mockUserService
                    .Setup(s => s.GetAllUsersAsync())
                    .ReturnsAsync(new List<UserModel>());

                var viewModel = new UsersViewModel(mockUserService.Object, mockDialogService.Object);

                // Set form data
                viewModel.Email = email;
                viewModel.FirstName = "Test";
                viewModel.LastName = "User";

                // Act
                viewModel.CreateUserCommand.Execute(null);
                Task.Delay(100).Wait();

                // Assert
                var isValidEmail = IsValidEmail(email);
                
                if (isValidEmail)
                {
                    // Valid email: service should be called, no error message
                    return (createCalled && string.IsNullOrEmpty(viewModel.ErrorMessage))
                        .Label($"Valid email '{email}' should call service (called={createCalled}, error={viewModel.ErrorMessage})");
                }
                else
                {
                    // Invalid email: service should NOT be called, error message should be set
                    return (!createCalled && !string.IsNullOrEmpty(viewModel.ErrorMessage))
                        .Label($"Invalid email '{email}' should not call service (called={createCalled}, error={viewModel.ErrorMessage})");
                }
            });
    }

    /// <summary>
    /// Validates if an email is in valid format.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        // Use the same regex pattern as the ViewModel
        var emailRegex = new System.Text.RegularExpressions.Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return emailRegex.IsMatch(email);
    }

    /// <summary>
    /// Generates various email strings including empty, invalid, and valid emails.
    /// </summary>
    private static Arbitrary<string> GenerateEmail()
    {
        // Empty emails
        var emptyGen = Gen.Constant(string.Empty);
        
        // Whitespace emails
        var whitespaceGen = Gen.Elements("", " ", "  ", "\t");
        
        // Invalid emails (no @)
        var noAtGen = Gen.Elements("test", "user", "admin", "invalid");
        
        // Invalid emails (multiple @)
        var multipleAtGen = Gen.Constant("test@test@example.com");
        
        // Invalid emails (no domain)
        var noDomainGen = Gen.Elements("user@", "test@", "admin@");
        
        // Invalid emails (no local part)
        var noLocalGen = Gen.Constant("@example.com");
        
        // Invalid emails (no dot in domain)
        var noDotGen = Gen.Elements("user@domain", "test@server");
        
        // Invalid emails (with spaces)
        var withSpacesGen = Gen.Elements("user @example.com", "user@ example.com", "user@example .com");
        
        // Valid emails
        var validGen = from local in Gen.Elements("user", "test", "admin", "john", "jane", "contact")
                       from domain in Gen.Elements("example", "test", "company", "domain")
                       from tld in Gen.Elements("com", "org", "net", "edu", "gov")
                       select $"{local}@{domain}.{tld}";
        
        return Arb.From(Gen.OneOf(
            emptyGen,
            whitespaceGen,
            noAtGen,
            multipleAtGen,
            noDomainGen,
            noLocalGen,
            noDotGen,
            withSpacesGen,
            validGen
        ));
    }
}
