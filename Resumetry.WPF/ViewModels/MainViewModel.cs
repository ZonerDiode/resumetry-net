using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Resumetry.Application.Interfaces;
using Resumetry.Views;

namespace Resumetry.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IJobApplicationService _jobApplicationService;
        private string _filterText = string.Empty;
        private ObservableCollection<JobApplicationViewModel> _jobApplications;
        private ObservableCollection<JobApplicationViewModel> _filteredJobApplications;
        private JobApplicationViewModel? _selectedJobApplication;

        public MainViewModel(IServiceProvider serviceProvider, IJobApplicationService jobApplicationService)
        {
            _serviceProvider = serviceProvider;
            _jobApplicationService = jobApplicationService;
            _jobApplications = [];
            _filteredJobApplications = [];

            NewApplicationCommand = new RelayCommand(_ => OpenNewApplicationForm());
            OpenEditApplicationFormCommand = new RelayCommand(_ => OpenEditApplicationForm(), _ => SelectedJobApplication != null);
            DeleteApplicationCommand = new RelayCommand(_ => DeleteApplication(), _ => SelectedJobApplication != null);
            ReportsCommand = new RelayCommand(_ => OpenReports());
            RefreshCommand = new RelayCommand(async _ => await LoadJobApplicationsAsync());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

            // Load initial data
            _ = LoadJobApplicationsAsync();
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
                var summaryDtos = await _jobApplicationService.GetAllAsync();

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
            var formViewModel = _serviceProvider.GetRequiredService<ApplicationFormViewModel>();
            var formWindow = new ApplicationFormWindow(formViewModel);
            if (formWindow.ShowDialog() == true)
            {
                // Refresh the list after adding
                _ = LoadJobApplicationsAsync();
            }
        }

        private async void OpenEditApplicationForm()
        {
            if (SelectedJobApplication == null) return;

            try
            {
                // Load the full job application detail DTO
                var detailDto = await _jobApplicationService.GetByIdAsync(SelectedJobApplication.Id);
                if (detailDto == null)
                {
                    System.Windows.MessageBox.Show("Job application not found.",
                        "Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                    return;
                }

                var formViewModel = _serviceProvider.GetRequiredService<ApplicationFormViewModel>();
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

        private async void DeleteApplication()
        {
            if (SelectedJobApplication == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete the application for {SelectedJobApplication.Company} - {SelectedJobApplication.Position}?\n\nThis action cannot be undone.",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes) return;

            try
            {
                // Delete the job application via service
                await _jobApplicationService.DeleteAsync(SelectedJobApplication.Id);

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

        private void OpenReports()
        {
            // TODO: Implement reports functionality
            System.Windows.MessageBox.Show("Reports functionality coming soon!", "Reports",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void OpenSettings()
        {
            var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
            var settingsWindow = new SettingsWindow(settingsViewModel);
            settingsWindow.ShowDialog();

            // Refresh the list after closing settings (in case data was imported)
            _ = LoadJobApplicationsAsync();
        }
    }
}
