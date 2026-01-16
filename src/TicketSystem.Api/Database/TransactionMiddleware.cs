namespace TicketSystem.Api.Database;

public class TransactionMiddleware
{
    private readonly RequestDelegate _next;

    public TransactionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, NHibernate.ISession session)
    {
        using var transaction = session.BeginTransaction();
        try
        {
            await _next(context);
            
            if (transaction.IsActive)
            {
                await transaction.CommitAsync();
            }
        }
        catch
        {
            if (transaction.IsActive)
            {
                await transaction.RollbackAsync();
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
