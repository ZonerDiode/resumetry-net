using System.Windows;
using Resumetry.ViewModels;

namespace Resumetry.Views
{
    public partial class ApplicationFormWindow : Window
    {
        private ApplicationFormViewModel ViewModel => (ApplicationFormViewModel)DataContext;

        public ApplicationFormWindow(ApplicationFormViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Validate())
            {
                MessageBox.Show("Please fill in all required fields (Company, Role, Salary, and Description).",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var success = await ViewModel.SaveAsync();
            if (success)
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
