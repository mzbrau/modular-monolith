namespace TicketSystem.Api.Database;

public class TransactionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TransactionMiddleware> _logger;

    public TransactionMiddleware(RequestDelegate next, ILogger<TransactionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, NHibernate.ISession session)
    {
        var requestPath = context.Request.Path;
        _logger.LogDebug("Beginning database transaction for request: {RequestPath}", requestPath);
        
        using var transaction = session.BeginTransaction();
        try
        {
            await _next(context);
            
            if (transaction.IsActive)
            {
                await transaction.CommitAsync();
                _logger.LogDebug("Transaction committed successfully for request: {RequestPath}", requestPath);
            }
        }
        catch (Exception ex)
        {
            if (transaction.IsActive)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Transaction rolled back due to error for request: {RequestPath}", requestPath);
            }
            throw;
        }
    }
}

public static class TransactionMiddlewareExtensions
{
    public static IApplicationBuilder UseNHibernateTransaction(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TransactionMiddleware>();
    }
}
