using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Moq;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Client.Wpf.ViewModels;
using TeamModel = TicketSystem.Client.Wpf.Models.Team;

namespace TicketSystem.Client.Wpf.Tests.ViewModels;

/// <summary>
/// Property-based tests for TeamsViewModel class.
/// Feature: wpf-ticket-client
/// </summary>
public class TeamsViewModelPropertyTests
{
    /// <summary>
    /// Property 6: Team Name Validation
    /// 
    /// **Validates: Requirements 8.6**
    /// 
    /// For any team creation or edit operation, if the name is empty or whitespace-only,
    /// the validation SHALL fail and prevent the operation.
    /// 
    /// This property test verifies that:
    /// 1. Empty team names are rejected
    /// 2. Whitespace-only team names are rejected
    /// 3. Valid team names are accepted
    /// 4. Error message is set when validation fails
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property6_TeamNameValidation_CreateTeam()
    {
        return Prop.ForAll(
            GenerateTeamName(),
            teamName =>
            {
                // Arrange
                var mockTeamService = new Mock<ITeamService>();
                var mockUserService = new Mock<IUserService>();
                var mockDialogService = new Mock<IDialogService>();
                mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                var createCalled = false;
                mockTeamService
                    .Setup(s => s.CreateTeamAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Callback(() => createCalled = true)
                    .ReturnsAsync("new-team-id");

                mockTeamService
                    .Setup(s => s.GetAllTeamsAsync())
                    .ReturnsAsync(new List<TeamModel>());

                var viewModel = new TeamsViewModel(
                    mockTeamService.Object,
                    mockUserService.Object,
                    mockDialogService.Object);

                // Set form data
                viewModel.TeamName = teamName;
                viewModel.TeamDescription = "Test Description";

                // Act
                viewModel.CreateTeamCommand.Execute(null);
                Task.Delay(100).Wait();

                // Assert
                var isValidName = !string.IsNullOrWhiteSpace(teamName);
                
                if (isValidName)
                {
                    // Valid name: service should be called, no error message
                    return (createCalled && string.IsNullOrEmpty(viewModel.ErrorMessage))
                        .Label($"Valid team name '{teamName}' should call service (called={createCalled}, error={viewModel.ErrorMessage})");
                }
                else
                {
                    // Invalid name: service should NOT be called, error message should be set
                    return (!createCalled && !string.IsNullOrEmpty(viewModel.ErrorMessage))
                        .Label($"Invalid team name '{teamName}' should not call service (called={createCalled}, error={viewModel.ErrorMessage})");
                }
            });
    }

    /// <summary>
    /// Property 6: Team Name Validation (Edit Operation)
    /// 
    /// **Validates: Requirements 8.6**
    /// 
    /// Tests team name validation for edit operations.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property6_TeamNameValidation_EditTeam()
    {
        return Prop.ForAll(
            GenerateTeamName(),
            teamName =>
            {
                // Arrange
                var mockTeamService = new Mock<ITeamService>();
                var mockUserService = new Mock<IUserService>();
                var mockDialogService = new Mock<IDialogService>();
                mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                var updateCalled = false;
                mockTeamService
                    .Setup(s => s.UpdateTeamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Callback(() => updateCalled = true)
                    .Returns(Task.CompletedTask);

                mockTeamService
                    .Setup(s => s.GetTeamAsync(It.IsAny<string>()))
                    .ReturnsAsync(new TeamModel
                    {
                        Id = "test-id",
                        Name = "Updated Team",
                        Description = "Updated Description",
                        CreatedDate = DateTime.UtcNow,
                        Members = new List<TeamMember>()
                    });

                var viewModel = new TeamsViewModel(
                    mockTeamService.Object,
                    mockUserService.Object,
                    mockDialogService.Object);

                // Add and select a team
                var team = new TeamModel
                {
                    Id = "test-id",
                    Name = "Original Team",
                    Description = "Original Description",
                    CreatedDate = DateTime.UtcNow,
                    Members = new List<TeamMember>()
                };
                viewModel.Teams.Add(team);
                viewModel.SelectedTeam = team;

                // Set form data
                viewModel.TeamName = teamName;
                viewModel.TeamDescription = "Test Description";

                // Act
                viewModel.EditTeamCommand.Execute(null);
                Task.Delay(100).Wait();

                // Assert
                var isValidName = !string.IsNullOrWhiteSpace(teamName);
                
                if (isValidName)
                {
                    // Valid name: service should be called, no error message
                    return (updateCalled && string.IsNullOrEmpty(viewModel.ErrorMessage))
                        .Label($"Valid team name '{teamName}' should call service (called={updateCalled}, error={viewModel.ErrorMessage})");
                }
                else
                {
                    // Invalid name: service should NOT be called, error message should be set
                    return (!updateCalled && !string.IsNullOrEmpty(viewModel.ErrorMessage))
                        .Label($"Invalid team name '{teamName}' should not call service (called={updateCalled}, error={viewModel.ErrorMessage})");
                }
            });
    }

    /// <summary>
    /// Generates various team name strings including empty, whitespace, and valid names.
    /// </summary>
    private static Arbitrary<string> GenerateTeamName()
    {
        var emptyGen = Gen.Constant(string.Empty);
        var whitespaceGen = Gen.Elements("", " ", "  ", "\t", "\n", "   \t  ");
        var validGen = Arb.Default.String().Generator.Where(s => !string.IsNullOrWhiteSpace(s));
        
        return Arb.From(Gen.OneOf(emptyGen, whitespaceGen, validGen));
    }
}
