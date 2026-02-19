using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Resumetry.Application.Services;
using Resumetry.Domain.Enums;

namespace Resumetry.ViewModels;

/// <summary>
/// ViewModel for the Add Status dialog.
/// </summary>
public partial class AddStatusDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private DateTime _occurred = DateTime.Now;

    [ObservableProperty]
    private StatusEnum _selectedStatus;

    /// <summary>The statuses available for selection, as determined by the state engine.</summary>
    public StatusEnum[] AvailableStatuses { get; }

    /// <summary>Raised when the dialog should close; the bool indicates whether the user confirmed.</summary>
    public event Action<bool>? CloseRequested;

    public AddStatusDialogViewModel(IEnumerable<StatusEnum> currentStatuses)
    {
        AvailableStatuses = StatusStateEngine.AvailableStatuses(currentStatuses.ToList());
        SelectedStatus = AvailableStatuses.Length > 0 ? AvailableStatuses[0] : StatusEnum.Applied;
    }

    [RelayCommand]
    private void Confirm() => CloseRequested?.Invoke(true);

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);
}
