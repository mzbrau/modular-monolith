using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Grpc.Core;
using Moq;
using TicketSystem.Issue.Infrastructure.Grpc;
using IssueServiceClient = TicketSystem.Issue.Infrastructure.Grpc.IssueService.IssueServiceClient;
using IssueServiceImpl = TicketSystem.Client.Wpf.Services.IssueService;

namespace TicketSystem.Client.Wpf.Tests.Services;

/// <summary>
/// Property-based tests for IssueService gRPC error handling.
/// Feature: wpf-ticket-client
/// </summary>
public class IssueServicePropertyTests
{
    /// <summary>
    /// Generator for gRPC status codes that represent different error types.
    /// </summary>
    private static class StatusCodeGenerators
    {
        public static Gen<StatusCode> NetworkErrorCodes() =>
            Gen.Elements(StatusCode.Unavailable, StatusCode.DeadlineExceeded, StatusCode.Cancelled);

        public static Gen<StatusCode> ValidationErrorCodes() =>
            Gen.Elements(StatusCode.InvalidArgument, StatusCode.FailedPrecondition, StatusCode.OutOfRange);

        public static Gen<StatusCode> UnexpectedErrorCodes() =>
            Gen.Elements(StatusCode.Internal, StatusCode.Unknown, StatusCode.DataLoss, StatusCode.Aborted);

        public static Gen<StatusCode> AllErrorCodes() =>
            Gen.OneOf(NetworkErrorCodes(), ValidationErrorCodes(), UnexpectedErrorCodes());
    }

    /// <summary>
    /// Property 8: gRPC Error Handling
    /// 
    /// **Validates: Requirements 2.4, 2.5, 4.7, 6.5, 15.1, 15.2, 15.3**
    /// 
    /// For any gRPC call that fails, the application SHALL catch the exception, display an appropriate
    /// error message to the user based on the error type (network, validation, or unexpected), and log
    /// the error details.
    /// 
    /// This property test verifies that:
    /// 1. Network errors (Unavailable, DeadlineExceeded, Cancelled) produce connection failure messages
    /// 2. Validation errors (InvalidArgument, FailedPrecondition, OutOfRange) produce validation messages
    /// 3. Unexpected errors (Internal, Unknown, DataLoss, Aborted) produce generic error messages
    /// 4. All RpcExceptions are caught and transformed to InvalidOperationException with meaningful messages
    /// 5. The original RpcException is preserved as the inner exception
    /// </summary>
    [Property(MaxTest = 100)]
    public void Property8_GrpcErrorHandling_GetAllIssuesAsync_NetworkErrors()
    {
        Prop.ForAll(
            StatusCodeGenerators.NetworkErrorCodes().ToArbitrary(),
            Arb.Default.NonEmptyString().Generator.ToArbitrary(),
            (statusCode, errorDetail) =>
            {
                // Arrange
                var mockClient = new Mock<IssueServiceClient>();
                var rpcException = new RpcException(new Status(statusCode, errorDetail.Get));

                mockClient
                    .Setup(c => c.ListIssuesAsync(
                        It.IsAny<ListIssuesRequest>(),
                        It.IsAny<Grpc.Core.Metadata>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<CancellationToken>()))
                    .Throws(rpcException);

                var service = new IssueServiceImpl(mockClient.Object);

                // Act
                Func<Task> act = async () => await service.GetAllIssuesAsync();

                // Assert
                var exception = act.Should().ThrowAsync<InvalidOperationException>().Result.Which;

                // Verify the error message indicates a connection failure
                var messageContainsConnectionError = exception.Message.Contains("connect", StringComparison.OrdinalIgnoreCase) ||
                                                     exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                                                     exception.Message.Contains("server", StringComparison.OrdinalIgnoreCase);

                // Verify the inner exception is the original RpcException
                var innerExceptionIsRpcException = exception.InnerException is RpcException;

                return (messageContainsConnectionError && innerExceptionIsRpcException)
                    .Label($"Network error ({statusCode}) should produce connection failure message. " +
                           $"Message: '{exception.Message}', HasRpcInnerException: {innerExceptionIsRpcException}");
            }).QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Property 8: gRPC Error Handling - Validation Errors
    /// 
    /// **Validates: Requirements 2.4, 2.5, 4.7, 6.5, 15.1, 15.2, 15.3**
    /// 
    /// Tests that validation errors produce appropriate validation messages.
    /// </summary>
    [Property(MaxTest = 100)]
    public void Property8_GrpcErrorHandling_CreateIssueAsync_ValidationErrors()
    {
        Prop.ForAll(
            StatusCodeGenerators.ValidationErrorCodes().ToArbitrary(),
            Arb.Default.NonEmptyString().Generator.ToArbitrary(),
            (statusCode, errorDetail) =>
            {
                // Arrange
                var mockClient = new Mock<IssueServiceClient>();
                var rpcException = new RpcException(new Status(statusCode, errorDetail.Get));

                mockClient
                    .Setup(c => c.CreateIssueAsync(
                        It.IsAny<CreateIssueRequest>(),
                        It.IsAny<Grpc.Core.Metadata>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<CancellationToken>()))
                    .Throws(rpcException);

                var service = new IssueServiceImpl(mockClient.Object);

                // Act
                Func<Task> act = async () => await service.CreateIssueAsync("Test Title", "Test Description", 1, null);

                // Assert
                var exception = act.Should().ThrowAsync<InvalidOperationException>().Result.Which;

                // Verify the error message indicates a validation failure
                var messageContainsValidationError = exception.Message.Contains("validation", StringComparison.OrdinalIgnoreCase) ||
                                                     exception.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase);

                // Verify the error detail is included in the message
                var messageContainsDetail = exception.Message.Contains(errorDetail.Get, StringComparison.OrdinalIgnoreCase);

                // Verify the inner exception is the original RpcException
                var innerExceptionIsRpcException = exception.InnerException is RpcException;

                return (messageContainsValidationError && messageContainsDetail && innerExceptionIsRpcException)
                    .Label($"Validation error ({statusCode}) should produce validation message with detail. " +
                           $"Message: '{exception.Message}', Detail: '{errorDetail.Get}', HasRpcInnerException: {innerExceptionIsRpcException}");
            }).QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Property 8: gRPC Error Handling - Unexpected Errors
    /// 
    /// **Validates: Requirements 2.4, 2.5, 4.7, 6.5, 15.1, 15.2, 15.3**
    /// 
    /// Tests that unexpected errors produce generic error messages with details.
    /// </summary>
    [Property(MaxTest = 100)]
    public void Property8_GrpcErrorHandling_UpdateIssueAsync_UnexpectedErrors()
    {
        Prop.ForAll(
            StatusCodeGenerators.UnexpectedErrorCodes().ToArbitrary(),
            Arb.Default.NonEmptyString().Generator.ToArbitrary(),
            (statusCode, errorDetail) =>
            {
                // Arrange
                var mockClient = new Mock<IssueServiceClient>();
                var rpcException = new RpcException(new Status(statusCode, errorDetail.Get));

                mockClient
                    .Setup(c => c.UpdateIssueAsync(
                        It.IsAny<UpdateIssueRequest>(),
                        It.IsAny<Grpc.Core.Metadata>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<CancellationToken>()))
                    .Throws(rpcException);

                var service = new IssueServiceImpl(mockClient.Object);

                // Act
                Func<Task> act = async () => await service.UpdateIssueAsync("issue-1", "Test Title", "Test Description", 1, null);

                // Assert
                var exception = act.Should().ThrowAsync<InvalidOperationException>().Result.Which;

                // Verify the error message contains failure information
                var messageContainsFailure = exception.Message.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                                            exception.Message.Contains("error", StringComparison.OrdinalIgnoreCase);

                // Verify the error detail is included in the message
                var messageContainsDetail = exception.Message.Contains(errorDetail.Get, StringComparison.OrdinalIgnoreCase);

                // Verify the inner exception is the original RpcException
                var innerExceptionIsRpcException = exception.InnerException is RpcException;

                return (messageContainsFailure && messageContainsDetail && innerExceptionIsRpcException)
                    .Label($"Unexpected error ({statusCode}) should produce error message with detail. " +
                           $"Message: '{exception.Message}', Detail: '{errorDetail.Get}', HasRpcInnerException: {innerExceptionIsRpcException}");
            }).QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Property 8: gRPC Error Handling - Delete Operation
    /// 
    /// **Validates: Requirements 2.4, 2.5, 4.7, 6.5, 15.1, 15.2, 15.3**
    /// 
    /// Tests that all error types are handled correctly for delete operations.
    /// </summary>
    [Property(MaxTest = 100)]
    public void Property8_GrpcErrorHandling_DeleteIssueAsync_AllErrorTypes()
    {
        Prop.ForAll(
            StatusCodeGenerators.AllErrorCodes().ToArbitrary(),
            Arb.Default.NonEmptyString().Generator.ToArbitrary(),
            (statusCode, errorDetail) =>
            {
                // Arrange
                var mockClient = new Mock<IssueServiceClient>();
                var rpcException = new RpcException(new Status(statusCode, errorDetail.Get));

                mockClient
                    .Setup(c => c.DeleteIssueAsync(
                        It.IsAny<DeleteIssueRequest>(),
                        It.IsAny<Grpc.Core.Metadata>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<CancellationToken>()))
                    .Throws(rpcException);

                var service = new IssueServiceImpl(mockClient.Object);

                // Act
                Func<Task> act = async () => await service.DeleteIssueAsync("issue-1");

                // Assert
                var exception = act.Should().ThrowAsync<InvalidOperationException>().Result.Which;

                // Verify an InvalidOperationException is thrown
                var isInvalidOperationException = exception is InvalidOperationException;

                // Verify the error message is not empty
                var hasErrorMessage = !string.IsNullOrWhiteSpace(exception.Message);

                // Verify the inner exception is the original RpcException
                var innerExceptionIsRpcException = exception.InnerException is RpcException;

                // Verify the inner exception has the correct status code
                var innerRpcException = exception.InnerException as RpcException;
                var hasCorrectStatusCode = innerRpcException?.StatusCode == statusCode;

                return (isInvalidOperationException && hasErrorMessage && innerExceptionIsRpcException && hasCorrectStatusCode)
                    .Label($"Error ({statusCode}) should be caught and wrapped in InvalidOperationException. " +
                           $"Message: '{exception.Message}', HasRpcInnerException: {innerExceptionIsRpcException}, " +
                           $"StatusCode: {innerRpcException?.StatusCode}");
            }).QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Property 8: gRPC Error Handling - NotFound Errors
    /// 
    /// **Validates: Requirements 2.4, 2.5, 4.7, 6.5, 15.1, 15.2, 15.3**
    /// 
    /// Tests that NotFound errors are handled appropriately (GetIssueAsync returns null, others throw).
    /// </summary>
    [Property(MaxTest = 100)]
    public void Property8_GrpcErrorHandling_GetIssueAsync_NotFoundReturnsNull()
    {
        Prop.ForAll(
            Arb.Default.NonEmptyString().Generator.ToArbitrary(),
            errorDetail =>
            {
                // Arrange
                var mockClient = new Mock<IssueServiceClient>();
                var rpcException = new RpcException(new Status(StatusCode.NotFound, errorDetail.Get));

                mockClient
                    .Setup(c => c.GetIssueAsync(
                        It.IsAny<GetIssueRequest>(),
                        It.IsAny<Grpc.Core.Metadata>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<CancellationToken>()))
                    .Throws(rpcException);

                var service = new IssueServiceImpl(mockClient.Object);

                // Act
                var result = service.GetIssueAsync("issue-1").Result;

                // Assert
                // GetIssueAsync should return null for NotFound errors, not throw
                return (result == null)
                    .Label($"GetIssueAsync should return null for NotFound errors. Result: {result}");
            }).QuickCheckThrowOnFailure();
    }
}
