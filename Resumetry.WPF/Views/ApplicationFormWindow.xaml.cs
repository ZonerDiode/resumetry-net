using System.Windows;
using Resumetry.ViewModels;

namespace Resumetry.Views
{
    public partial class ApplicationFormWindow : Window
    {
        private ApplicationFormViewModel ViewModel => (ApplicationFormViewModel)DataContext;

        public ApplicationFormWindow()
        {
            InitializeComponent();
            DataContext = new ApplicationFormViewModel();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Validate())
            {
                MessageBox.Show("Please fill in all required fields (Company and Role).",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // TODO: Save to database using repository
            MessageBox.Show("Job application will be saved once repository integration is complete.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
