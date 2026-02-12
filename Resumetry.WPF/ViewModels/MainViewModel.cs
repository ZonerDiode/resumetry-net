using System.Collections.ObjectModel;
using System.Windows.Input;
using Resumetry.Application.Interfaces;
using Resumetry.WPF.Services;

namespace Resumetry.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IScopedRunner _scopedRunner;
        private readonly IDialogService _dialogService;
        private string _filterText = string.Empty;
        private ObservableCollection<JobApplicationViewModel> _jobApplications;
        private ObservableCollection<JobApplicationViewModel> _filteredJobApplications;
        private JobApplicationViewModel? _selectedJobApplication;

        public MainViewModel(IScopedRunner scopedRunner, IDialogService dialogService)
        {
            _scopedRunner = scopedRunner ?? throw new ArgumentNullException(nameof(scopedRunner));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
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
                _dialogService.ShowError($"Error loading initial data: {ex.Message}", "Startup Error");
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
                var summaryDtos = await _scopedRunner.RunAsync<IJobApplicationService, IEnumerable<Application.DTOs.JobApplicationSummaryDto>>(
                    async svc => await svc.GetAllAsync());

                foreach (var summaryDto in summaryDtos)
                {
                    _jobApplications.Add(new JobApplicationViewModel(summaryDto));
                }

                ApplyFilter();
                OnPropertyChanged(nameof(TotalCount));
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading job applications: {ex.Message}", "Error");
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
            if (_dialogService.ShowApplicationForm())
            {
                // Refresh the list after adding
                ExecuteAsyncSafe(LoadJobApplicationsAsync, ex =>
                {
                    _dialogService.ShowError($"Error refreshing data: {ex.Message}", "Error");
                });
            }
        }

        private async Task OpenEditApplicationFormAsync()
        {
            if (SelectedJobApplication == null) return;

            try
            {
                // Load the full job application detail DTO
                var detailDto = await _scopedRunner.RunAsync<IJobApplicationService, Application.DTOs.JobApplicationDetailDto?>(
                    async svc => await svc.GetByIdAsync(SelectedJobApplication.Id));

                if (detailDto == null)
                {
                    _dialogService.ShowError("Job application not found.", "Error");
                    return;
                }

                if (_dialogService.ShowApplicationForm(vm => vm.LoadExistingJobApplication(detailDto)))
                {
                    // Refresh the list after editing
                    await LoadJobApplicationsAsync();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading job application: {ex.Message}", "Error");
            }
        }

        private async Task DeleteApplicationAsync()
        {
            if (SelectedJobApplication == null) return;

            if (!_dialogService.Confirm(
                $"Are you sure you want to delete the application for {SelectedJobApplication.Company} - {SelectedJobApplication.Position}?\n\nThis action cannot be undone.",
                "Confirm Delete"))
            {
                return;
            }

            try
            {
                await _scopedRunner.RunAsync<IJobApplicationService>(
                    async svc => await svc.DeleteAsync(SelectedJobApplication.Id));

                // Refresh the list
                await LoadJobApplicationsAsync();
            }
            catch (KeyNotFoundException)
            {
                _dialogService.ShowError("Job application not found.", "Error");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error deleting job application: {ex.Message}", "Error");
            }
        }

        private void OpenReports()
        {
            // TODO: Implement reports functionality
            _dialogService.ShowInfo("Reports functionality coming soon!", "Reports");
        }

        private void OpenSettings()
        {
            _dialogService.ShowSettings();

            // Refresh the list after closing settings (in case data was imported)
            ExecuteAsyncSafe(LoadJobApplicationsAsync, ex =>
            {
                _dialogService.ShowError($"Error refreshing data: {ex.Message}", "Error");
            });
        }
    }
}
