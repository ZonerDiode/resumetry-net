using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Resumetry.Application.Interfaces;
using Resumetry.Views;

namespace Resumetry.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private string _filterText = string.Empty;
        private ObservableCollection<JobApplicationViewModel> _jobApplications;
        private ObservableCollection<JobApplicationViewModel> _filteredJobApplications;
        private JobApplicationViewModel? _selectedJobApplication;

        public MainViewModel(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _jobApplications = [];
            _filteredJobApplications = [];

            NewApplicationCommand = new RelayCommand(_ => OpenNewApplicationForm());
            OpenEditApplicationFormCommand = new AsyncRelayCommand(OpenEditApplicationFormAsync, () => SelectedJobApplication != null);
            DeleteApplicationCommand = new AsyncRelayCommand(DeleteApplicationAsync, () => SelectedJobApplication != null);
            ReportsCommand = new RelayCommand(_ => OpenReports());
            RefreshCommand = new AsyncRelayCommand(LoadJobApplicationsAsync);
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

            // Load initial data
            ExecuteAsyncSafe(LoadJobApplicationsAsync, ex =>
            {
                System.Windows.MessageBox.Show($"Error loading initial data: {ex.Message}",
                    "Startup Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            });
        }

        public ObservableCollection<JobApplicationViewModel> FilteredJobApplications
        {
            get => _filteredJobApplications;
            set => SetProperty(ref _filteredJobApplications, value);
        }

        public JobApplicationViewModel? SelectedJobApplication
        {
            get => _selectedJobApplication;
            set => SetProperty(ref _selectedJobApplication, value);
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    ApplyFilter();
                }
            }
        }

        public int TotalCount => _jobApplications.Count;

        public ICommand NewApplicationCommand { get; }
        public ICommand OpenEditApplicationFormCommand { get; }
        public ICommand DeleteApplicationCommand { get; }
        public ICommand ReportsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        private async Task LoadJobApplicationsAsync()
        {
            _jobApplications.Clear();
            FilteredJobApplications.Clear();

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var jobApplicationService = scope.ServiceProvider.GetRequiredService<IJobApplicationService>();

                var summaryDtos = await jobApplicationService.GetAllAsync();

                foreach (var summaryDto in summaryDtos)
                {
                    _jobApplications.Add(new JobApplicationViewModel(summaryDto));
                }

                ApplyFilter();
                OnPropertyChanged(nameof(TotalCount));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading job applications: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            FilteredJobApplications.Clear();

            var filtered = string.IsNullOrWhiteSpace(FilterText)
                ? _jobApplications
                : _jobApplications.Where(ja =>
                    ja.Company.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

            foreach (var item in filtered)
            {
                FilteredJobApplications.Add(item);
            }
        }

        private void OpenNewApplicationForm()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var formViewModel = scope.ServiceProvider.GetRequiredService<ApplicationFormViewModel>();
            var formWindow = new ApplicationFormWindow(formViewModel);
            if (formWindow.ShowDialog() == true)
            {
                // Refresh the list after adding
                ExecuteAsyncSafe(LoadJobApplicationsAsync, ex =>
                {
                    System.Windows.MessageBox.Show($"Error refreshing data: {ex.Message}",
                        "Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                });
            }
        }

        private async Task OpenEditApplicationFormAsync()
        {
            if (SelectedJobApplication == null) return;

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var jobApplicationService = scope.ServiceProvider.GetRequiredService<IJobApplicationService>();
                var formViewModel = scope.ServiceProvider.GetRequiredService<ApplicationFormViewModel>();

                // Load the full job application detail DTO
                var detailDto = await jobApplicationService.GetByIdAsync(SelectedJobApplication.Id);
                if (detailDto == null)
                {
                    System.Windows.MessageBox.Show("Job application not found.",
                        "Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                    return;
                }

                formViewModel.LoadExistingJobApplication(detailDto);

                var formWindow = new ApplicationFormWindow(formViewModel);
                if (formWindow.ShowDialog() == true)
                {
                    // Refresh the list after editing
                    await LoadJobApplicationsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading job application: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private async Task DeleteApplicationAsync()
        {
            if (SelectedJobApplication == null) return;

            if (!ConfirmDelete(SelectedJobApplication))
                return;

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var jobApplicationService = scope.ServiceProvider.GetRequiredService<IJobApplicationService>();

                // Delete the job application via service
                await jobApplicationService.DeleteAsync(SelectedJobApplication.Id);

                // Refresh the list
                await LoadJobApplicationsAsync();
            }
            catch (KeyNotFoundException)
            {
                System.Windows.MessageBox.Show("Job application not found.",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting job application: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Shows a confirmation dialog for deleting a job application.
        /// Virtual to allow testing without UI interaction.
        /// </summary>
        protected virtual bool ConfirmDelete(JobApplicationViewModel app)
        {
            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete the application for {app.Company} - {app.Position}?\n\nThis action cannot be undone.",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            return result == System.Windows.MessageBoxResult.Yes;
        }

        private void OpenReports()
        {
            // TODO: Implement reports functionality
            System.Windows.MessageBox.Show("Reports functionality coming soon!", "Reports",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void OpenSettings()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var settingsViewModel = scope.ServiceProvider.GetRequiredService<SettingsViewModel>();
            var settingsWindow = new SettingsWindow(settingsViewModel);
            settingsWindow.ShowDialog();

            // Refresh the list after closing settings (in case data was imported)
            ExecuteAsyncSafe(LoadJobApplicationsAsync, ex =>
            {
                System.Windows.MessageBox.Show($"Error refreshing data: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            });
        }
    }
}
