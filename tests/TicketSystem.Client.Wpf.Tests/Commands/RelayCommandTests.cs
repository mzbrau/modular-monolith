using FluentAssertions;
using TicketSystem.Client.Wpf.Commands;

namespace TicketSystem.Client.Wpf.Tests.Commands;

/// <summary>
/// Unit tests for RelayCommand class.
/// Tests Requirements 14.5 - Command binding to UI controls using ICommand implementations.
/// </summary>
public class RelayCommandTests
{
    [Fact]
    public void Constructor_WithNullExecute_ShouldThrowArgumentNullException()
    {
        // Arrange
        Action<object?> execute = null!;

        // Act
        Action act = () => new RelayCommand(execute);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("execute");
    }

    [Fact]
    public void Constructor_WithValidExecute_ShouldNotThrow()
    {
        // Arrange
        Action<object?> execute = _ => { };

        // Act
        Action act = () => new RelayCommand(execute);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullCanExecute_ShouldNotThrow()
    {
        // Arrange
        Action<object?> execute = _ => { };
        Func<object?, bool>? canExecute = null;

        // Act
        Action act = () => new RelayCommand(execute, canExecute);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Execute_ShouldInvokeExecuteDelegate()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(_ => executed = true);

        // Act
        command.Execute(null);

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public void Execute_ShouldPassParameterToDelegate()
    {
        // Arrange
        object? receivedParameter = null;
        var command = new RelayCommand(param => receivedParameter = param);
        var expectedParameter = "test parameter";

        // Act
        command.Execute(expectedParameter);

        // Assert
        receivedParameter.Should().Be(expectedParameter);
    }

    [Fact]
    public void Execute_WithNullParameter_ShouldPassNullToDelegate()
    {
        // Arrange
        object? receivedParameter = "not null";
        var command = new RelayCommand(param => receivedParameter = param);

        // Act
        command.Execute(null);

        // Assert
        receivedParameter.Should().BeNull();
    }

    [Fact]
    public void Execute_MultipleInvocations_ShouldInvokeDelegateEachTime()
    {
        // Arrange
        var executionCount = 0;
        var command = new RelayCommand(_ => executionCount++);

        // Act
        command.Execute(null);
        command.Execute(null);
        command.Execute(null);

        // Assert
        executionCount.Should().Be(3);
    }

    [Fact]
    public void CanExecute_WithNoCanExecuteDelegate_ShouldReturnTrue()
    {
        // Arrange
        var command = new RelayCommand(_ => { });

        // Act
        var result = command.CanExecute(null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanExecute_WithCanExecuteDelegateReturningTrue_ShouldReturnTrue()
    {
        // Arrange
        var command = new RelayCommand(_ => { }, _ => true);

        // Act
        var result = command.CanExecute(null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanExecute_WithCanExecuteDelegateReturningFalse_ShouldReturnFalse()
    {
        // Arrange
        var command = new RelayCommand(_ => { }, _ => false);

        // Act
        var result = command.CanExecute(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanExecute_ShouldPassParameterToDelegate()
    {
        // Arrange
        object? receivedParameter = null;
        var command = new RelayCommand(
            _ => { },
            param =>
            {
                receivedParameter = param;
                return true;
            });
        var expectedParameter = "test parameter";

        // Act
        command.CanExecute(expectedParameter);

        // Assert
        receivedParameter.Should().Be(expectedParameter);
    }

    [Fact]
    public void CanExecute_WithConditionalLogic_ShouldReturnCorrectValue()
    {
        // Arrange
        var command = new RelayCommand(
            _ => { },
            param => param is int value && value > 0);

        // Act & Assert
        command.CanExecute(5).Should().BeTrue();
        command.CanExecute(0).Should().BeFalse();
        command.CanExecute(-1).Should().BeFalse();
        command.CanExecute("string").Should().BeFalse();
        command.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void RaiseCanExecuteChanged_ShouldRaiseCanExecuteChangedEvent()
    {
        // Arrange
        var command = new RelayCommand(_ => { });
        var eventRaised = false;

        command.CanExecuteChanged += (sender, args) =>
        {
            eventRaised = true;
        };

        // Act
        command.RaiseCanExecuteChanged();

        // Assert
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void RaiseCanExecuteChanged_WithNoSubscribers_ShouldNotThrow()
    {
        // Arrange
        var command = new RelayCommand(_ => { });

        // Act
        Action act = () => command.RaiseCanExecuteChanged();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RaiseCanExecuteChanged_ShouldPassCommandAsSender()
    {
        // Arrange
        var command = new RelayCommand(_ => { });
        object? receivedSender = null;

        command.CanExecuteChanged += (sender, args) =>
        {
            receivedSender = sender;
        };

        // Act
        command.RaiseCanExecuteChanged();

        // Assert
        receivedSender.Should().Be(command);
    }

    [Fact]
    public void RaiseCanExecuteChanged_MultipleSubscribers_ShouldNotifyAll()
    {
        // Arrange
        var command = new RelayCommand(_ => { });
        var subscriber1Notified = false;
        var subscriber2Notified = false;
        var subscriber3Notified = false;

        command.CanExecuteChanged += (sender, args) => subscriber1Notified = true;
        command.CanExecuteChanged += (sender, args) => subscriber2Notified = true;
        command.CanExecuteChanged += (sender, args) => subscriber3Notified = true;

        // Act
        command.RaiseCanExecuteChanged();

        // Assert
        subscriber1Notified.Should().BeTrue();
        subscriber2Notified.Should().BeTrue();
        subscriber3Notified.Should().BeTrue();
    }

    [Fact]
    public void Command_WithChangingCanExecuteCondition_ShouldReflectChanges()
    {
        // Arrange
        var canExecute = true;
        var command = new RelayCommand(_ => { }, _ => canExecute);

        // Act & Assert - Initially can execute
        command.CanExecute(null).Should().BeTrue();

        // Change condition
        canExecute = false;
        command.CanExecute(null).Should().BeFalse();

        // Notify UI of change
        var eventRaised = false;
        command.CanExecuteChanged += (sender, args) => eventRaised = true;
        command.RaiseCanExecuteChanged();
        eventRaised.Should().BeTrue();

        // Verify new state
        command.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void Execute_WhenCanExecuteIsFalse_ShouldStillExecute()
    {
        // Note: ICommand.Execute does not check CanExecute automatically.
        // It's the responsibility of the caller (typically WPF binding) to check CanExecute.
        
        // Arrange
        var executed = false;
        var command = new RelayCommand(_ => executed = true, _ => false);

        // Act
        command.Execute(null);

        // Assert
        executed.Should().BeTrue("Execute should run even if CanExecute returns false");
        command.CanExecute(null).Should().BeFalse("CanExecute should still return false");
    }

    [Fact]
    public void Command_IntegrationScenario_ShouldWorkCorrectly()
    {
        // Arrange - Simulate a save command that's only enabled when data is valid
        var data = "";
        var saveExecuted = false;
        var saveCommand = new RelayCommand(
            execute: _ =>
            {
                saveExecuted = true;
            },
            canExecute: _ => !string.IsNullOrWhiteSpace(data));

        // Act & Assert - Initially cannot execute (empty data)
        saveCommand.CanExecute(null).Should().BeFalse();

        // User enters data
        data = "Some data";
        saveCommand.CanExecute(null).Should().BeTrue();

        // Notify UI that CanExecute changed
        var canExecuteChangedRaised = false;
        saveCommand.CanExecuteChanged += (s, e) => canExecuteChangedRaised = true;
        saveCommand.RaiseCanExecuteChanged();
        canExecuteChangedRaised.Should().BeTrue();

        // Execute the command
        saveCommand.Execute(null);
        saveExecuted.Should().BeTrue();
    }
}
