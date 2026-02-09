using System.Collections.ObjectModel;
using System.Windows.Input;
using Resumetry.Views;

namespace Resumetry.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _filterText = string.Empty;
        private ObservableCollection<JobApplicationViewModel> _jobApplications;
        private ObservableCollection<JobApplicationViewModel> _filteredJobApplications;

        public MainViewModel()
        {
            _jobApplications = new ObservableCollection<JobApplicationViewModel>();
            _filteredJobApplications = new ObservableCollection<JobApplicationViewModel>();

            NewApplicationCommand = new RelayCommand(_ => OpenNewApplicationForm());
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
        public ICommand ReportsCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadJobApplicationsAsync()
        {
            // TODO: Load from database using repository
            // For now, this is a placeholder
            _jobApplications.Clear();
            FilteredJobApplications.Clear();

            // Simulate loading
            await Task.Delay(100);

            ApplyFilter();
            OnPropertyChanged(nameof(TotalCount));
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
            var formWindow = new ApplicationFormWindow();
            if (formWindow.ShowDialog() == true)
            {
                // Refresh the list after adding
                _ = LoadJobApplicationsAsync();
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
