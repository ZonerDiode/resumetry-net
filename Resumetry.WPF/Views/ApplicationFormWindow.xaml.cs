using System.Windows;
using Resumetry.ViewModels;
using Microsoft.Web.WebView2.Core;

namespace Resumetry.Views
{
    public partial class ApplicationFormWindow : Window
    {
        private ApplicationFormViewModel ViewModel => (ApplicationFormViewModel)DataContext;
        private bool _isWebViewInitialized = false;

        public ApplicationFormWindow(ApplicationFormViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Subscribe to property changes to update WebView2
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Description))
            {
                UpdateWebViewContent();
            }
        }

        private async void DescriptionWebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                _isWebViewInitialized = true;
                UpdateWebViewContent();
            }
        }

        private void UpdateWebViewContent()
        {
            if (_isWebViewInitialized && DescriptionWebView.CoreWebView2 != null)
            {
                var htmlContent = WrapHtmlContent(ViewModel.Description);
                DescriptionWebView.NavigateToString(htmlContent);
            }
        }

        private string WrapHtmlContent(string content)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            font-size: 14px;
            line-height: 1.6;
            color: #333;
            padding: 12px;
            margin: 0;
            background-color: white;
        }}
        ul, ol {{
            margin-left: 20px;
            padding-left: 0;
        }}
        li {{
            margin-bottom: 8px;
        }}
        strong {{
            font-weight: 600;
        }}
        br {{
            margin-bottom: 8px;
        }}
    </style>
</head>
<body>
    {content}
</body>
</html>";
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize WebView2
            await DescriptionWebView.EnsureCoreWebView2Async(null);
        }
    }
}
