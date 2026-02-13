using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Resumetry.Application.Interfaces;
using Resumetry.WPF.Services;

namespace Resumetry.ViewModels
{
    public partial class MainViewModel(IScopedRunner scopedRunner, IDialogService dialogService) : ViewModelBase
    {
        private readonly ObservableCollection<JobApplicationViewModel> _jobApplications = [];

        public int TotalCount => _jobApplications.Count;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenEditApplicationFormCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteApplicationCommand))]
        private JobApplicationViewModel? _selectedJobApplication;

        [ObservableProperty]
        private ObservableCollection<JobApplicationViewModel> _filteredJobApplications = [];

        [ObservableProperty]
        private string _filterText = string.Empty;

        private bool CanModifyApplication() => SelectedJobApplication != null;

        partial void OnFilterTextChanged(string value)
        {
            ApplyFilter();
        }

        [RelayCommand]
        private async Task LoadJobApplicationsAsync()
        {
            _jobApplications.Clear();
            FilteredJobApplications.Clear();

            try
            {
                var summaryDtos = await scopedRunner.RunAsync<IJobApplicationService, IEnumerable<Application.DTOs.JobApplicationSummaryDto>>(
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
                dialogService.ShowError($"Error loading job applications: {ex.Message}", "Error");
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

        [RelayCommand]
        private void NewApplication()
        {
            if (dialogService.ShowApplicationForm())
            {
                // Refresh the list after adding
                ExecuteAsyncSafe(LoadJobApplicationsAsync, ex =>
                {
                    dialogService.ShowError($"Error refreshing data: {ex.Message}", "Error");
                });
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifyApplication))]
        private async Task OpenEditApplicationFormAsync()
        {
            if (SelectedJobApplication == null) return;

            try
            {
                // Load the full job application detail DTO
                var detailDto = await scopedRunner.RunAsync<IJobApplicationService, Application.DTOs.JobApplicationDetailDto?>(
                    async svc => await svc.GetByIdAsync(SelectedJobApplication.Id));

                if (detailDto == null)
                {
                    dialogService.ShowError("Job application not found.", "Error");
                    return;
                }

                if (dialogService.ShowApplicationForm(vm => vm.LoadExistingJobApplication(detailDto)))
                {
                    // Refresh the list after editing
                    await LoadJobApplicationsAsync();
                }
            }
            catch (Exception ex)
            {
                dialogService.ShowError($"Error loading job application: {ex.Message}", "Error");
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifyApplication))]
        private async Task DeleteApplicationAsync()
        {
            if (SelectedJobApplication == null) return;

            if (!dialogService.Confirm(
                $"Are you sure you want to delete the application for {SelectedJobApplication.Company} - {SelectedJobApplication.Position}?\n\nThis action cannot be undone.",
                "Confirm Delete"))
            {
                return;
            }

            try
            {
                await scopedRunner.RunAsync<IJobApplicationService>(
                    async svc => await svc.DeleteAsync(SelectedJobApplication.Id));

                // Refresh the list
                await LoadJobApplicationsAsync();
            }
            catch (KeyNotFoundException)
            {
                dialogService.ShowError("Job application not found.", "Error");
            }
            catch (Exception ex)
            {
                dialogService.ShowError($"Error deleting job application: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private void Reports()
        {
            // TODO: Implement reports functionality
            dialogService.ShowInfo("Reports functionality coming soon!", "Reports");
        }

        [RelayCommand]
        private void OpenSettings()
        {
            dialogService.ShowSettings();

            // Refresh the list after closing settings (in case data was imported)
            ExecuteAsyncSafe(LoadJobApplicationsAsync, ex =>
            {
                dialogService.ShowError($"Error refreshing data: {ex.Message}", "Error");
            });
        }
    }
}
