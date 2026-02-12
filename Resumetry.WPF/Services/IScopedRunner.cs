namespace Resumetry.WPF.Services;

/// <summary>
/// Provides a mechanism to run operations within a service scope.
/// </summary>
public interface IScopedRunner
{
    /// <summary>
    /// Runs an asynchronous operation with a scoped service.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    /// <param name="operation">The operation to execute with the service.</param>
    Task RunAsync<TService>(Func<TService, Task> operation) where TService : notnull;

    /// <summary>
    /// Runs an asynchronous operation with a scoped service and returns a result.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    /// <typeparam name="TResult">The type of result to return.</typeparam>
    /// <param name="operation">The operation to execute with the service.</param>
    /// <returns>The result of the operation.</returns>
    Task<TResult> RunAsync<TService, TResult>(Func<TService, Task<TResult>> operation) where TService : notnull;
}
