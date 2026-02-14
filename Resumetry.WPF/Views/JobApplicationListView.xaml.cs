using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Resumetry.ViewModels;

namespace Resumetry.Views
{
    /// <summary>
    /// Interaction logic for JobApplicationListView.xaml
    /// </summary>
    public partial class JobApplicationListView : UserControl
    {
        private JobApplicationListViewModel? ViewModel => DataContext as JobApplicationListViewModel;

        public JobApplicationListView()
        {
            InitializeComponent();
        }

        private async void JobApplicationListView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                // Register ViewModel for messages
                WeakReferenceMessenger.Default.RegisterAll(ViewModel);

                // Load data
                await ViewModel.LoadJobApplicationsCommand.ExecuteAsync(null);
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel?.SelectedJobApplication != null)
            {
                ViewModel.OpenEditApplicationFormCommand.Execute(null);
            }
        }
    }
}
