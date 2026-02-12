using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Resumetry.ViewModels
{
    /// <summary>
    /// An asynchronous implementation of ICommand that safely executes async operations
    /// and prevents re-entrant execution.
    /// </summary>
    public class AsyncRelayCommand : ICommand, INotifyPropertyChanged
    {
        private readonly Func<object?, Task> _execute;
        private readonly Func<bool>? _canExecute;
        private readonly Action<Exception>? _onError;
        private bool _isRunning;
        private Task? _runningTask;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Indicates whether the command is currently executing.
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Creates a new AsyncRelayCommand that executes an async action.
        /// </summary>
        /// <param name="execute">The async action to execute.</param>
        /// <param name="onError">Optional error handler for exceptions.</param>
        public AsyncRelayCommand(Func<Task> execute, Action<Exception>? onError = null)
            : this(WrapWithNullCheck(execute), null, onError)
        {
        }

        /// <summary>
        /// Creates a new AsyncRelayCommand that executes an async action with a parameter.
        /// </summary>
        /// <param name="execute">The async action to execute with a parameter.</param>
        /// <param name="onError">Optional error handler for exceptions.</param>
        public AsyncRelayCommand(Func<object?, Task> execute, Action<Exception>? onError = null)
            : this(execute, null, onError)
        {
        }

        /// <summary>
        /// Creates a new AsyncRelayCommand with a canExecute predicate.
        /// </summary>
        /// <param name="execute">The async action to execute.</param>
        /// <param name="canExecute">Predicate to determine if the command can execute.</param>
        /// <param name="onError">Optional error handler for exceptions.</param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute, Action<Exception>? onError = null)
            : this(WrapWithNullCheck(execute), canExecute, onError)
        {
        }

        private static Func<object?, Task> WrapWithNullCheck(Func<Task> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return _ => execute();
        }

        /// <summary>
        /// Creates a new AsyncRelayCommand with a parameter and canExecute predicate.
        /// </summary>
        /// <param name="execute">The async action to execute with a parameter.</param>
        /// <param name="canExecute">Predicate to determine if the command can execute.</param>
        /// <param name="onError">Optional error handler for exceptions.</param>
        public AsyncRelayCommand(Func<object?, Task> execute, Func<bool>? canExecute, Action<Exception>? onError = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _onError = onError;
        }

        public bool CanExecute(object? parameter)
        {
            // Prevent re-entrant execution
            if (IsRunning)
                return false;

            return _canExecute == null || _canExecute();
        }

        public async void Execute(object? parameter)
        {
            // Prevent re-entrant execution
            if (IsRunning)
                return;

            _runningTask = ExecuteAsync(parameter);
            await _runningTask;
        }

        private async Task ExecuteAsync(object? parameter)
        {
            try
            {
                IsRunning = true;
                await _execute(parameter);
            }
            catch (Exception ex)
            {
                _onError?.Invoke(ex);
            }
            finally
            {
                IsRunning = false;
                _runningTask = null;
            }
        }

        /// <summary>
        /// Gets the currently running task, if any. Useful for testing.
        /// </summary>
        internal Task? RunningTask => _runningTask;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
