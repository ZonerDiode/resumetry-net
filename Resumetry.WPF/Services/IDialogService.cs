using Resumetry.ViewModels;

namespace Resumetry.WPF.Services;

/// <summary>
/// Provides message box functionality.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">The title of the dialog.</param>
    /// <returns>True if Yes was clicked; otherwise, false.</returns>
    bool Confirm(string message, string title);

    /// <summary>
    /// Shows an error message dialog.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <param name="title">The title of the dialog.</param>
    void ShowError(string message, string title);

    /// <summary>
    /// Shows an information message dialog.
    /// </summary>
    /// <param name="message">The information message to display.</param>
    /// <param name="title">The title of the dialog.</param>
    void ShowInfo(string message, string title);
}
