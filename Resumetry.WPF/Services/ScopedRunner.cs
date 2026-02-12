using Microsoft.Extensions.DependencyInjection;

namespace Resumetry.WPF.Services;

/// <summary>
/// Runs operations within a service scope.
/// </summary>
public class ScopedRunner(IServiceScopeFactory serviceScopeFactory) : IScopedRunner
{
    /// <inheritdoc/>
    public async Task RunAsync<TService>(Func<TService, Task> operation) where TService : notnull
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        await operation(service);
    }

    /// <inheritdoc/>
    public async Task<TResult> RunAsync<TService, TResult>(Func<TService, Task<TResult>> operation) where TService : notnull
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return await operation(service);
    }
}
