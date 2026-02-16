using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Resumetry.Application.DTOs;
using Resumetry.Application.Interfaces;
using Resumetry.WPF.Services;

namespace Resumetry.ViewModels
{
    /// <summary>
    /// ViewModel for the Sankey Report view that visualizes application status flow.
    /// </summary>
    public partial class SankeyReportViewModel(IScopedRunner scopedRunner, INavigationService navigationService) : ViewModelBase
    {
        [ObservableProperty]
        private ImmutableList<SankeyReportData> _reportData = [];

        [ObservableProperty]
        private int _totalApplications;

        [ObservableProperty]
        private bool _isLoaded;

        /// <summary>
        /// Loads the Sankey report data from the service.
        /// </summary>
        [RelayCommand]
        private async Task LoadReportAsync()
        {
            IsLoaded = false;
            try
            {
                var data = await scopedRunner.RunAsync<ISankeyReportService, ImmutableList<SankeyReportData>>(
                    async svc => await svc.GenerateSankeyReport());

                ReportData = data;

                // Calculate total applications from flows starting with "Applied"
                TotalApplications = data
                    .Where(r => r.From == "Applied")
                    .Sum(r => r.Count);
            }
            catch (Exception)
            {
                // Silently handle errors for now - could add error display later
                ReportData = [];
                TotalApplications = 0;
            }
            finally
            {
                IsLoaded = true;
            }
        }

        /// <summary>
        /// Navigates back to the home view.
        /// </summary>
        [RelayCommand]
        private void GoBack()
        {
            navigationService.NavigateToHome();
        }
    }
}
