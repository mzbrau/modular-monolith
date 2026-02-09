namespace TicketSystem.Testing.Common.Builders;

/// <summary>
/// Base class for fluent test data builders.
/// </summary>
/// <typeparam name="TBuilder">The builder type (for fluent returns).</typeparam>
/// <typeparam name="TResult">The type being built.</typeparam>
public abstract class BuilderBase<TBuilder, TResult> 
    where TBuilder : BuilderBase<TBuilder, TResult>
{
    protected TBuilder This => (TBuilder)this;
    
    public abstract TResult Build();
    
    public abstract Task<TResult> CreateAsync();
}
