using Microsoft.Win32;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Interfaces;
using System.Windows.Input;

namespace Resumetry.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IImportService _importService;
        private readonly IUnitOfWork _unitOfWork;
        private string _statusMessage = string.Empty;

        public SettingsViewModel(IImportService importService, IUnitOfWork unitOfWork)
        {
            _importService = importService;
            _unitOfWork = unitOfWork;

            ImportFromJsonCommand = new RelayCommand(async _ => await ImportFromJsonAsync());
            ExportToJsonCommand = new RelayCommand(_ => ExportToJson(), _ => false); // Disabled for now
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand ImportFromJsonCommand { get; }
        public ICommand ExportToJsonCommand { get; }

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

                var jobApplications = await _importService.ImportFromJsonAsync(openFileDialog.FileName);
                var jobApplicationsList = jobApplications.ToList();

                int importedCount = 0;
                foreach (var jobApplication in jobApplicationsList)
                {
                    await _unitOfWork.JobApplications.AddAsync(jobApplication);
                    importedCount++;
                }

                await _unitOfWork.SaveChangesAsync();

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
