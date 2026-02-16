using System.Windows;
using System.Windows.Controls;
using Resumetry.ViewModels;

namespace Resumetry.Views
{
    /// <summary>
    /// Interaction logic for SankeyReportView.xaml
    /// </summary>
    public partial class SankeyReportView : UserControl
    {
        private SankeyReportViewModel? ViewModel => DataContext as SankeyReportViewModel;

        public SankeyReportView()
        {
            InitializeComponent();
        }

        private async void SankeyReportView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadReportCommand.ExecuteAsync(null);
            }
        }
    }
}
