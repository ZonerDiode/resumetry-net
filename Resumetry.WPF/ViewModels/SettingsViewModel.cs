using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Interfaces;
using Resumetry.WPF.Messages;
using Resumetry.WPF.Services;

namespace Resumetry.ViewModels
{
    public partial class SettingsViewModel(IImportService importService, IExportService exportService, IUnitOfWork unitOfWork, INavigationService navigationService, IDialogService dialogService) : ViewModelBase
    {

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [RelayCommand]
        private async Task ImportFromLegacyJsonAsync()
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

                var jobApplications = await importService.ImportFromJsonAsync(openFileDialog.FileName);
                var jobApplicationsList = jobApplications.ToList();

                int importedCount = 0;
                foreach (var jobApplication in jobApplicationsList)
                {
                    await unitOfWork.JobApplications.AddAsync(jobApplication);
                    importedCount++;
                }

                await unitOfWork.SaveChangesAsync();

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

                var jobApplications = await unitOfWork.JobApplications.GetAllAsync();
                var jobApplicationsList = jobApplications.ToList();

                await exportService.ExportToJsonAsync(jobApplicationsList, saveFileDialog.FileName);

                StatusMessage = $"Successfully exported {jobApplicationsList.Count} application(s) to {saveFileDialog.FileName}";
                dialogService.ShowInfo($"Successfully exported {jobApplicationsList.Count} application(s)", "Export Success");
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
