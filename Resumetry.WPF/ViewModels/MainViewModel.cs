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
        private readonly IUnitOfWork _unitOfWork;
        private string _filterText = string.Empty;
        private ObservableCollection<JobApplicationViewModel> _jobApplications;
        private ObservableCollection<JobApplicationViewModel> _filteredJobApplications;
        private JobApplicationViewModel? _selectedJobApplication;

        public MainViewModel(IServiceProvider serviceProvider, IUnitOfWork unitOfWork)
        {
            _serviceProvider = serviceProvider;
            _unitOfWork = unitOfWork;
            _jobApplications = new ObservableCollection<JobApplicationViewModel>();
            _filteredJobApplications = new ObservableCollection<JobApplicationViewModel>();

            NewApplicationCommand = new RelayCommand(_ => OpenNewApplicationForm());
            OpenEditApplicationFormCommand = new RelayCommand(_ => OpenEditApplicationForm(), _ => SelectedJobApplication != null);
            ReportsCommand = new RelayCommand(_ => OpenReports());
            RefreshCommand = new RelayCommand(async _ => await LoadJobApplicationsAsync());

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
        public ICommand ReportsCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadJobApplicationsAsync()
        {
            _jobApplications.Clear();
            FilteredJobApplications.Clear();

            try
            {
                var jobApplications = await _unitOfWork.JobApplications.GetAllAsync();

                foreach (var jobApplication in jobApplications)
                {
                    _jobApplications.Add(new JobApplicationViewModel(jobApplication));
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
                    ja.Company.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                    ja.Position.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

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
            if (SelectedJobApplication == null || !SelectedJobApplication.Id.HasValue) return;

            try
            {
                // Load the full job application with all related data
                var jobApplication = await _unitOfWork.JobApplications.GetByIdAsync(SelectedJobApplication.Id.Value);
                if (jobApplication == null)
                {
                    System.Windows.MessageBox.Show("Job application not found.",
                        "Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                    return;
                }

                var formViewModel = _serviceProvider.GetRequiredService<ApplicationFormViewModel>();
                formViewModel.LoadExistingJobApplication(jobApplication);

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

        private void OpenReports()
        {
            // TODO: Implement reports functionality
            System.Windows.MessageBox.Show("Reports functionality coming soon!", "Reports",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}
