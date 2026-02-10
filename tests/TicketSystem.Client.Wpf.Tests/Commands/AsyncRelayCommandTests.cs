using FluentAssertions;
using TicketSystem.Client.Wpf.Commands;

namespace TicketSystem.Client.Wpf.Tests.Commands;

/// <summary>
/// Unit tests for AsyncRelayCommand class.
/// Tests Requirements:
/// - 13.1: Asynchronous execution of gRPC calls
/// - 13.5: Use of async/await patterns for I/O operations
/// - 14.5: Command binding to UI controls using ICommand implementations
/// </summary>
public class AsyncRelayCommandTests
{
    [Fact]
    public void Constructor_WithNullExecute_ShouldThrowArgumentNullException()
    {
        // Arrange
        Func<object?, Task> execute = null!;

        // Act
        Action act = () => new AsyncRelayCommand(execute);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("execute");
    }

    [Fact]
    public void Constructor_WithValidExecute_ShouldNotThrow()
    {
        // Arrange
        Func<object?, Task> execute = _ => Task.CompletedTask;

        // Act
        Action act = () => new AsyncRelayCommand(execute);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullCanExecute_ShouldNotThrow()
    {
        // Arrange
        Func<object?, Task> execute = _ => Task.CompletedTask;
        Func<object?, bool>? canExecute = null;

        // Act
        Action act = () => new AsyncRelayCommand(execute, canExecute);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task Execute_ShouldInvokeExecuteDelegate()
    {
        // Arrange
        var executed = false;
        var command = new AsyncRelayCommand(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        // Act
        command.Execute(null);
        await Task.Delay(50); // Give async operation time to complete

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_ShouldPassParameterToDelegate()
    {
        // Arrange
        object? receivedParameter = null;
        var command = new AsyncRelayCommand(param =>
        {
            receivedParameter = param;
            return Task.CompletedTask;
        });
        var expectedParameter = "test parameter";

        // Act
        command.Execute(expectedParameter);
        await Task.Delay(50); // Give async operation time to complete

        // Assert
        receivedParameter.Should().Be(expectedParameter);
    }

    [Fact]
    public async Task Execute_WithNullParameter_ShouldPassNullToDelegate()
    {
        // Arrange
        object? receivedParameter = "not null";
        var command = new AsyncRelayCommand(param =>
        {
            receivedParameter = param;
            return Task.CompletedTask;
        });

        // Act
        command.Execute(null);
        await Task.Delay(50); // Give async operation time to complete

        // Assert
        receivedParameter.Should().BeNull();
    }

    [Fact]
    public void CanExecute_WithNoCanExecuteDelegate_ShouldReturnTrue()
    {
        // Arrange
        var command = new AsyncRelayCommand(_ => Task.CompletedTask);

        // Act
        var result = command.CanExecute(null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanExecute_WithCanExecuteDelegateReturningTrue_ShouldReturnTrue()
    {
        // Arrange
        var command = new AsyncRelayCommand(_ => Task.CompletedTask, _ => true);

        // Act
        var result = command.CanExecute(null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanExecute_WithCanExecuteDelegateReturningFalse_ShouldReturnFalse()
    {
        // Arrange
        var command = new AsyncRelayCommand(_ => Task.CompletedTask, _ => false);

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
        var command = new AsyncRelayCommand(
            _ => Task.CompletedTask,
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
    public void RaiseCanExecuteChanged_ShouldRaiseCanExecuteChangedEvent()
    {
        // Arrange
        var command = new AsyncRelayCommand(_ => Task.CompletedTask);
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
        var command = new AsyncRelayCommand(_ => Task.CompletedTask);

        // Act
        Action act = () => command.RaiseCanExecuteChanged();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task CanExecute_WhileExecuting_ShouldReturnFalse()
    {
        // Arrange - Create a command with a long-running operation
        var tcs = new TaskCompletionSource<bool>();
        var command = new AsyncRelayCommand(_ => tcs.Task);

        // Act - Start execution
        command.Execute(null);
        await Task.Delay(10); // Give it time to start

        // Assert - Should not be able to execute while running
        command.CanExecute(null).Should().BeFalse("command should prevent concurrent execution");

        // Cleanup - Complete the task
        tcs.SetResult(true);
        await Task.Delay(50); // Give it time to complete
    }

    [Fact]
    public async Task CanExecute_AfterExecutionCompletes_ShouldReturnTrue()
    {
        // Arrange
        var command = new AsyncRelayCommand(_ => Task.CompletedTask);

        // Act - Execute and wait for completion
        command.Execute(null);
        await Task.Delay(50); // Give async operation time to complete

        // Assert - Should be able to execute again
        command.CanExecute(null).Should().BeTrue("command should be executable after completion");
    }

    [Fact]
    public async Task Execute_PreventsConcurrentExecution()
    {
        // Arrange - Create a command with a controlled async operation
        var tcs = new TaskCompletionSource<bool>();
        var executionCount = 0;
        var command = new AsyncRelayCommand(async _ =>
        {
            executionCount++;
            await tcs.Task;
        });

        // Act - Try to execute multiple times
        command.Execute(null);
        await Task.Delay(10); // Give first execution time to start
        command.Execute(null); // This should be prevented
        command.Execute(null); // This should also be prevented

        // Assert - Only one execution should have started
        executionCount.Should().Be(1, "concurrent executions should be prevented");

        // Cleanup
        tcs.SetResult(true);
        await Task.Delay(50);
    }

    [Fact]
    public async Task Execute_RaisesCanExecuteChanged_WhenExecutionStarts()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var command = new AsyncRelayCommand(_ => tcs.Task);
        var eventRaisedCount = 0;

        command.CanExecuteChanged += (sender, args) =>
        {
            eventRaisedCount++;
        };

        // Act
        command.Execute(null);
        await Task.Delay(10); // Give it time to start

        // Assert - Event should be raised when execution starts
        eventRaisedCount.Should().BeGreaterOrEqualTo(1, "CanExecuteChanged should be raised when execution starts");

        // Cleanup
        tcs.SetResult(true);
        await Task.Delay(50);
    }

    [Fact]
    public async Task Execute_RaisesCanExecuteChanged_WhenExecutionCompletes()
    {
        // Arrange
        var command = new AsyncRelayCommand(_ => Task.CompletedTask);
        var eventRaisedCount = 0;

        command.CanExecuteChanged += (sender, args) =>
        {
            eventRaisedCount++;
        };

        // Act
        command.Execute(null);
        await Task.Delay(50); // Give async operation time to complete

        // Assert - Event should be raised when execution starts and completes
        eventRaisedCount.Should().BeGreaterOrEqualTo(2, "CanExecuteChanged should be raised when execution starts and completes");
    }

    [Fact]
    public async Task Execute_WithException_ShouldStillResetExecutingFlag()
    {
        // Arrange
        var command = new AsyncRelayCommand(_ => Task.FromException(new InvalidOperationException("Test exception")));

        // Act - Execute and expect exception
        try
        {
            command.Execute(null);
            await Task.Delay(50); // Give async operation time to complete
        }
        catch
        {
            // Exception is expected
        }

        // Assert - Should be able to execute again after exception
        command.CanExecute(null).Should().BeTrue("command should be executable again after exception");
    }

    [Fact]
    public async Task Execute_WithException_ShouldRaiseCanExecuteChanged()
    {
        // Arrange
        var command = new AsyncRelayCommand(_ => Task.FromException(new InvalidOperationException("Test exception")));
        var eventRaisedCount = 0;

        command.CanExecuteChanged += (sender, args) =>
        {
            eventRaisedCount++;
        };

        // Act - Execute and expect exception
        try
        {
            command.Execute(null);
            await Task.Delay(50); // Give async operation time to complete
        }
        catch
        {
            // Exception is expected
        }

        // Assert - Event should be raised even when exception occurs
        eventRaisedCount.Should().BeGreaterOrEqualTo(2, "CanExecuteChanged should be raised even when exception occurs");
    }

    [Fact]
    public async Task Execute_WhenCanExecuteIsFalse_ShouldNotExecute()
    {
        // Arrange
        var executed = false;
        var command = new AsyncRelayCommand(
            _ =>
            {
                executed = true;
                return Task.CompletedTask;
            },
            _ => false);

        // Act
        command.Execute(null);
        await Task.Delay(50); // Give async operation time to complete

        // Assert
        executed.Should().BeFalse("Execute should not run when CanExecute returns false");
        command.CanExecute(null).Should().BeFalse("CanExecute should still return false");
    }

    [Fact]
    public async Task Execute_WithCustomCanExecuteLogic_ShouldRespectCondition()
    {
        // Arrange
        var canExecute = true;
        var executionCount = 0;
        var command = new AsyncRelayCommand(
            _ =>
            {
                executionCount++;
                return Task.CompletedTask;
            },
            _ => canExecute);

        // Act & Assert - Initially can execute
        command.CanExecute(null).Should().BeTrue();
        command.Execute(null);
        await Task.Delay(50);
        executionCount.Should().Be(1);

        // Change condition
        canExecute = false;
        command.CanExecute(null).Should().BeFalse();
        command.Execute(null);
        await Task.Delay(50);
        executionCount.Should().Be(1, "execution count should not increase when CanExecute is false");
    }

    [Fact]
    public async Task Command_IntegrationScenario_ShouldWorkCorrectly()
    {
        // Arrange - Simulate an async save command
        var data = "";
        var saveExecuted = false;
        var saveCommand = new AsyncRelayCommand(
            execute: async _ =>
            {
                await Task.Delay(10); // Simulate async operation
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
        
        // During execution, should not be able to execute again
        await Task.Delay(5);
        saveCommand.CanExecute(null).Should().BeFalse("should prevent concurrent execution");

        // Wait for completion
        await Task.Delay(50);
        saveExecuted.Should().BeTrue();
        saveCommand.CanExecute(null).Should().BeTrue("should be executable again after completion");
    }

    [Fact]
    public async Task Execute_MultipleSequentialExecutions_ShouldAllComplete()
    {
        // Arrange
        var executionCount = 0;
        var command = new AsyncRelayCommand(async _ =>
        {
            await Task.Delay(10);
            executionCount++;
        });

        // Act - Execute multiple times sequentially
        command.Execute(null);
        await Task.Delay(50);
        
        command.Execute(null);
        await Task.Delay(50);
        
        command.Execute(null);
        await Task.Delay(50);

        // Assert
        executionCount.Should().Be(3, "all sequential executions should complete");
    }
}
