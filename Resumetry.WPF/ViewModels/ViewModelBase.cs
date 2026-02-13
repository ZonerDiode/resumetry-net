using CommunityToolkit.Mvvm.ComponentModel;

namespace Resumetry.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// Executes an async action safely in a fire-and-forget manner.
        /// This is intended for async operations initiated from constructors or synchronous event handlers
        /// where awaiting is not possible. Exceptions are caught and handled via the optional error handler.
        /// </summary>
        /// <param name="action">The async action to execute.</param>
        /// <param name="onError">Optional error handler for exceptions. If null, exceptions are silently swallowed.</param>
        protected async void ExecuteAsyncSafe(Func<Task> action, Action<Exception>? onError = null)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }
    }
}
