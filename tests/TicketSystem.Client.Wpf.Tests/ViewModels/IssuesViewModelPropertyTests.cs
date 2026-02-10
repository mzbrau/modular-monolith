using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Moq;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Client.Wpf.ViewModels;
using IssueModel = TicketSystem.Client.Wpf.Models.Issue;

namespace TicketSystem.Client.Wpf.Tests.ViewModels;

/// <summary>
/// Property-based tests for IssuesViewModel class.
/// Feature: wpf-ticket-client
/// </summary>
public class IssuesViewModelPropertyTests
{
    /// <summary>
    /// Property 10: Loading Indicator Lifecycle
    /// 
    /// **Validates: Requirements 13.2, 13.4**
    /// 
    /// For any asynchronous operation, the application SHALL display a loading indicator when the operation starts
    /// and hide it when the operation completes (successfully or with error).
    /// 
    /// This property test verifies that:
    /// 1. IsLoading is set to true when an async operation starts
    /// 2. IsLoading is set to false when the operation completes successfully
    /// 3. IsLoading is set to false when the operation fails with an exception
    /// 4. This holds for ANY async operation in the ViewModel
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property10_LoadingIndicatorLifecycle_LoadIssuesSuccess()
    {
        return Prop.ForAll(
            GenerateIssueList(),
            issues =>
            {
                // Arrange
                var mockIssueService = new Mock<IIssueService>();
                var mockUserService = new Mock<IUserService>();
                var mockTeamService = new Mock<ITeamService>();
                var mockDialogService = new Mock<IDialogService>();
                mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                mockIssueService
                    .Setup(s => s.GetAllIssuesAsync())
                    .ReturnsAsync((IEnumerable<IssueModel>)issues);

                var viewModel = new IssuesViewModel(
                    mockIssueService.Object,
                    mockUserService.Object,
                    mockTeamService.Object,
                    mockDialogService.Object);

                var isLoadingStates = new List<bool>();

                // Track IsLoading changes
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(IssuesViewModel.IsLoading))
                    {
                        isLoadingStates.Add(viewModel.IsLoading);
                    }
                };

                // Act
                viewModel.LoadIssuesCommand.Execute(null);
                
                // Wait for async operation to complete
                Task.Delay(100).Wait();

                // Assert
                // IsLoading should have been set to true, then false
                var result = isLoadingStates.Count >= 2 &&
                       isLoadingStates[0] == true &&
                       isLoadingStates[isLoadingStates.Count - 1] == false &&
                       viewModel.IsLoading == false;

                return result.Label($"IsLoading lifecycle: states={string.Join(",", isLoadingStates)}, final={viewModel.IsLoading}");
            });
    }

    /// <summary>
    /// Property 10: Loading Indicator Lifecycle (Error Case)
    /// 
    /// **Validates: Requirements 13.2, 13.4**
    /// 
    /// Tests that IsLoading is properly reset even when an operation fails.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property10_LoadingIndicatorLifecycle_LoadIssuesError()
    {
        return Prop.ForAll(
            Arb.Default.String().Generator.ToArbitrary(),
            errorMessage =>
            {
                // Arrange
                var mockIssueService = new Mock<IIssueService>();
                var mockUserService = new Mock<IUserService>();
                var mockTeamService = new Mock<ITeamService>();
                var mockDialogService = new Mock<IDialogService>();
                mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                mockIssueService
                    .Setup(s => s.GetAllIssuesAsync())
                    .ThrowsAsync(new Exception(errorMessage ?? "Test error"));

                var viewModel = new IssuesViewModel(
                    mockIssueService.Object,
                    mockUserService.Object,
                    mockTeamService.Object,
                    mockDialogService.Object);

                var isLoadingStates = new List<bool>();

                // Track IsLoading changes
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(IssuesViewModel.IsLoading))
                    {
                        isLoadingStates.Add(viewModel.IsLoading);
                    }
                };

                // Act
                try
                {
                    viewModel.LoadIssuesCommand.Execute(null);
                    Task.Delay(100).Wait();
                }
                catch
                {
                    // Expected - we're testing error handling
                }

                // Assert
                // IsLoading should have been set to true, then false (even on error)
                var result = isLoadingStates.Count >= 2 &&
                       isLoadingStates[0] == true &&
                       isLoadingStates[isLoadingStates.Count - 1] == false &&
                       viewModel.IsLoading == false;

                return result.Label($"IsLoading lifecycle on error: states={string.Join(",", isLoadingStates)}, final={viewModel.IsLoading}");
            });
    }

    /// <summary>
    /// Property 10: Loading Indicator Lifecycle (Create Issue)
    /// 
    /// **Validates: Requirements 13.2, 13.4**
    /// 
    /// Tests that IsLoading is properly managed during issue creation.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property10_LoadingIndicatorLifecycle_CreateIssue()
    {
        return Prop.ForAll(
            GenerateValidIssueData(),
            issueData =>
            {
                // Arrange
                var mockIssueService = new Mock<IIssueService>();
                var mockUserService = new Mock<IUserService>();
                var mockTeamService = new Mock<ITeamService>();
                var mockDialogService = new Mock<IDialogService>();
                mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                mockIssueService
                    .Setup(s => s.CreateIssueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime?>()))
                    .ReturnsAsync("new-issue-id");

                mockIssueService
                    .Setup(s => s.GetAllIssuesAsync())
                    .ReturnsAsync(new List<IssueModel>());

                var viewModel = new IssuesViewModel(
                    mockIssueService.Object,
                    mockUserService.Object,
                    mockTeamService.Object,
                    mockDialogService.Object);

                // Set form data
                viewModel.Title = issueData.Title;
                viewModel.Description = issueData.Description;
                viewModel.Priority = issueData.Priority;
                viewModel.DueDate = issueData.DueDate;

                var isLoadingStates = new List<bool>();

                // Track IsLoading changes
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(IssuesViewModel.IsLoading))
                    {
                        isLoadingStates.Add(viewModel.IsLoading);
                    }
                };

                // Act
                viewModel.CreateIssueCommand.Execute(null);
                Task.Delay(100).Wait();

                // Assert
                // IsLoading should have been set to true, then false
                var result = isLoadingStates.Count >= 2 &&
                       isLoadingStates[0] == true &&
                       isLoadingStates[isLoadingStates.Count - 1] == false &&
                       viewModel.IsLoading == false;

                return result.Label($"IsLoading lifecycle for CreateIssue: states={string.Join(",", isLoadingStates)}, final={viewModel.IsLoading}");
            });
    }

    /// <summary>
    /// Property 10: Loading Indicator Lifecycle (Delete Issue)
    /// 
    /// **Validates: Requirements 13.2, 13.4**
    /// 
    /// Tests that IsLoading is properly managed during issue deletion.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property10_LoadingIndicatorLifecycle_DeleteIssue()
    {
        return Prop.ForAll(
            GenerateIssue(),
            issue =>
            {
                // Arrange
                var mockIssueService = new Mock<IIssueService>();
                var mockUserService = new Mock<IUserService>();
                var mockTeamService = new Mock<ITeamService>();
                var mockDialogService = new Mock<IDialogService>();
                mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                mockIssueService
                    .Setup(s => s.DeleteIssueAsync(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

                var viewModel = new IssuesViewModel(
                    mockIssueService.Object,
                    mockUserService.Object,
                    mockTeamService.Object,
                    mockDialogService.Object);

                // Add issue to collection and select it
                viewModel.Issues.Add(issue);
                viewModel.SelectedIssue = issue;

                var isLoadingStates = new List<bool>();

                // Track IsLoading changes
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(IssuesViewModel.IsLoading))
                    {
                        isLoadingStates.Add(viewModel.IsLoading);
                    }
                };

                // Act
                viewModel.DeleteIssueCommand.Execute(null);
                Task.Delay(100).Wait();

                // Assert
                // IsLoading should have been set to true, then false
                var result = isLoadingStates.Count >= 2 &&
                       isLoadingStates[0] == true &&
                       isLoadingStates[isLoadingStates.Count - 1] == false &&
                       viewModel.IsLoading == false;

                return result.Label($"IsLoading lifecycle for DeleteIssue: states={string.Join(",", isLoadingStates)}, final={viewModel.IsLoading}");
            });
    }

    /// <summary>
    /// Generates a list of random issues for testing.
    /// </summary>
    private static Arbitrary<IEnumerable<IssueModel>> GenerateIssueList()
    {
        return Arb.From(
            from count in Gen.Choose(0, 10)
            from issues in Gen.ListOf(count, GenerateIssue().Generator)
            select (IEnumerable<IssueModel>)issues.ToList());
    }

    /// <summary>
    /// Generates a random issue for testing.
    /// </summary>
    private static Arbitrary<IssueModel> GenerateIssue()
    {
        return Arb.From(
            from id in Arb.Default.String().Generator
            from title in Arb.Default.String().Generator
            from description in Arb.Default.String().Generator
            from status in Gen.Elements(Enum.GetValues<IssueStatus>())
            from priority in Arb.Default.Int32().Generator
            from createdDate in Arb.Default.DateTime().Generator
            select new IssueModel
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Title = title ?? "Test Issue",
                Description = description ?? "Test Description",
                Status = status,
                Priority = priority,
                CreatedDate = createdDate,
                LastModifiedDate = createdDate
            });
    }

    /// <summary>
    /// Generates valid issue data for creation testing.
    /// </summary>
    private static Arbitrary<IssueData> GenerateValidIssueData()
    {
        return Arb.From(
            from title in Arb.Default.String().Generator.Where(s => !string.IsNullOrWhiteSpace(s))
            from description in Arb.Default.String().Generator
            from priority in Arb.Default.Int32().Generator
            from hasDueDate in Arb.Default.Bool().Generator
            from dueDate in Arb.Default.DateTime().Generator
            select new IssueData
            {
                Title = title ?? "Valid Title",
                Description = description ?? "",
                Priority = priority,
                DueDate = hasDueDate ? dueDate : null
            });
    }

    /// <summary>
    /// <summary>
    /// Helper class for issue data generation.
    /// </summary>
    private class IssueData
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// Property 5: Issue Title Validation
    /// 
    /// **Validates: Requirements 4.6**
    /// 
    /// For any issue creation or edit operation, if the title is empty or whitespace-only,
    /// the validation SHALL fail and prevent the operation.
    /// 
    /// This property test verifies that:
    /// 1. Empty titles are rejected
    /// 2. Whitespace-only titles are rejected
    /// 3. Valid titles are accepted
    /// 4. Error message is set when validation fails
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property5_IssueTitleValidation_CreateIssue()
    {
        return Prop.ForAll(
            GenerateTitle(),
            title =>
            {
                // Arrange
                var mockIssueService = new Mock<IIssueService>();
                var mockUserService = new Mock<IUserService>();
                var mockTeamService = new Mock<ITeamService>();
                var mockDialogService = new Mock<IDialogService>();
                mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                var createCalled = false;
                mockIssueService
                    .Setup(s => s.CreateIssueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime?>()))
                    .Callback(() => createCalled = true)
                    .ReturnsAsync("new-issue-id");

                mockIssueService
                    .Setup(s => s.GetAllIssuesAsync())
                    .ReturnsAsync(new List<IssueModel>());

                var viewModel = new IssuesViewModel(
                    mockIssueService.Object,
                    mockUserService.Object,
                    mockTeamService.Object,
                    mockDialogService.Object);

                // Set form data
                viewModel.Title = title;
                viewModel.Description = "Test Description";
                viewModel.Priority = 1;

                // Act
                viewModel.CreateIssueCommand.Execute(null);
                Task.Delay(100).Wait();

                // Assert
                var isValidTitle = !string.IsNullOrWhiteSpace(title);
                
                if (isValidTitle)
                {
                    // Valid title: service should be called, no error message
                    return (createCalled && string.IsNullOrEmpty(viewModel.ErrorMessage))
                        .Label($"Valid title '{title}' should call service (called={createCalled}, error={viewModel.ErrorMessage})");
                }
                else
                {
                    // Invalid title: service should NOT be called, error message should be set
                    return (!createCalled && !string.IsNullOrEmpty(viewModel.ErrorMessage))
                        .Label($"Invalid title '{title}' should not call service (called={createCalled}, error={viewModel.ErrorMessage})");
                }
            });
    }

    /// <summary>
    /// Property 5: Issue Title Validation (Edit Operation)
    /// 
    /// **Validates: Requirements 4.6**
    /// 
    /// Tests title validation for edit operations.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property5_IssueTitleValidation_EditIssue()
    {
        return Prop.ForAll(
            GenerateTitle(),
            title =>
            {
                // Arrange
                var mockIssueService = new Mock<IIssueService>();
                var mockUserService = new Mock<IUserService>();
                var mockTeamService = new Mock<ITeamService>();
                var mockDialogService = new Mock<IDialogService>();
                mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                var updateCalled = false;
                mockIssueService
                    .Setup(s => s.UpdateIssueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime?>()))
                    .Callback(() => updateCalled = true)
                    .Returns(Task.CompletedTask);

                mockIssueService
                    .Setup(s => s.GetIssueAsync(It.IsAny<string>()))
                    .ReturnsAsync(new IssueModel
                    {
                        Id = "test-id",
                        Title = "Updated Title",
                        Description = "Updated Description",
                        Status = IssueStatus.Open,
                        Priority = 1,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    });

                var viewModel = new IssuesViewModel(
                    mockIssueService.Object,
                    mockUserService.Object,
                    mockTeamService.Object,
                    mockDialogService.Object);

                // Add and select an issue
                var issue = new IssueModel
                {
                    Id = "test-id",
                    Title = "Original Title",
                    Description = "Original Description",
                    Status = IssueStatus.Open,
                    Priority = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };
                viewModel.Issues.Add(issue);
                viewModel.SelectedIssue = issue;

                // Set form data
                viewModel.Title = title;
                viewModel.Description = "Test Description";
                viewModel.Priority = 1;

                // Act
                viewModel.EditIssueCommand.Execute(null);
                Task.Delay(100).Wait();

                // Assert
                var isValidTitle = !string.IsNullOrWhiteSpace(title);
                
                if (isValidTitle)
                {
                    // Valid title: service should be called, no error message
                    return (updateCalled && string.IsNullOrEmpty(viewModel.ErrorMessage))
                        .Label($"Valid title '{title}' should call service (called={updateCalled}, error={viewModel.ErrorMessage})");
                }
                else
                {
                    // Invalid title: service should NOT be called, error message should be set
                    return (!updateCalled && !string.IsNullOrEmpty(viewModel.ErrorMessage))
                        .Label($"Invalid title '{title}' should not call service (called={updateCalled}, error={viewModel.ErrorMessage})");
                }
            });
    }

    /// <summary>
    /// Generates various title strings including empty, whitespace, and valid titles.
    /// </summary>
    private static Arbitrary<string> GenerateTitle()
    {
        var emptyGen = Gen.Constant(string.Empty);
        var whitespaceGen = Gen.Elements("", " ", "  ", "\t", "\n", "   \t  ");
        var validGen = Arb.Default.String().Generator.Where(s => !string.IsNullOrWhiteSpace(s));
        
        return Arb.From(Gen.OneOf(emptyGen, whitespaceGen, validGen));
    }
}
