using System.Windows;
using System.Windows.Input;
using Resumetry.ViewModels;

namespace Resumetry
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.SelectedJobApplication != null)
            {
                ViewModel.OpenEditApplicationFormCommand.Execute(null);
            }
        }
    }
}