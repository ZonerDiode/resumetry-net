using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Interfaces;

namespace Resumetry.ViewModels
{
    public partial class SettingsViewModel(IImportService importService, IUnitOfWork unitOfWork) : ViewModelBase
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
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error importing: {ex.Message}";
                System.Windows.MessageBox.Show($"Error importing JSON file: {ex.Message}",
                    "Import Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanExport))]
        private void ExportToJson()
        {
            // Placeholder for future implementation
            System.Windows.MessageBox.Show("Export functionality coming soon!",
                "Export",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }
}
