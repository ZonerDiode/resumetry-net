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
    public partial class SettingsViewModel(IImportService importService, IUnitOfWork unitOfWork, INavigationService navigationService, IDialogService dialogService) : ViewModelBase
    {

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        private bool CanExport() => false;

        [RelayCommand]
        private async Task ImportFromJsonAsync()
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

        [RelayCommand(CanExecute = nameof(CanExport))]
        private void ExportToJson()
        {
            // Placeholder for future implementation
            dialogService.ShowInfo("Export functionality coming soon!", "Export");
        }

        [RelayCommand]
        private void Close()
        {
            navigationService.NavigateToHome();
        }
    }
}
