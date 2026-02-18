using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Resumetry.Application.Enums;
using Resumetry.Application.Interfaces;
using Resumetry.WPF.Messages;
using Resumetry.WPF.Services;

namespace Resumetry.ViewModels
{
    public partial class SettingsViewModel(
        [FromKeyedServices(ImportType.Standard)] IImportService importService,
        [FromKeyedServices(ImportType.Legacy)] IImportService legacyImportService,
        IExportService exportService,
        INavigationService navigationService,
        IDialogService dialogService) : ViewModelBase
    {

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [RelayCommand]
        private async Task ImportFromJsonAsync() => await ImportCoreAsync(importService.ImportFromJsonAsync);

        [RelayCommand]
        private async Task ImportFromLegacyJsonAsync() => await ImportCoreAsync(legacyImportService.ImportFromJsonAsync);
        
        private async Task ImportCoreAsync(
            Func<string, CancellationToken, Task<int>> importFunc)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select JSON file to import"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                StatusMessage = "Importing...";

                int importedCount = await importFunc(openFileDialog.FileName, CancellationToken.None);

                StatusMessage = $"Successfully imported {importedCount} application(s)";

                // Send message to notify that data was imported
                WeakReferenceMessenger.Default.Send(new DataImportedMessage());
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error importing: {ex.Message}";
                dialogService.ShowError($"Error importing JSON file: {ex.Message}", "Import Error");
            }
        }

        [RelayCommand]
        private async Task ExportToJsonAsync()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Export to JSON",
                FileName = $"resumetry_export_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                StatusMessage = "Exporting...";

                int exportedCount = await exportService.ExportToJsonAsync(saveFileDialog.FileName);

                StatusMessage = $"Successfully exported {exportedCount} application(s) to {saveFileDialog.FileName}";
                dialogService.ShowInfo($"Successfully exported {exportedCount} application(s)", "Export Success");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting: {ex.Message}";
                dialogService.ShowError($"Error exporting to JSON file: {ex.Message}", "Export Error");
            }
        }

        [RelayCommand]
        private void Close()
        {
            navigationService.NavigateToHome();
        }
    }
}
