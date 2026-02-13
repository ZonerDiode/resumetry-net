using System.Windows;
using Resumetry.ViewModels;
using Microsoft.Web.WebView2.Core;

namespace Resumetry.Views
{
    public partial class ApplicationFormWindow : Window
    {
        private ApplicationFormViewModel ViewModel => (ApplicationFormViewModel)DataContext;
        private bool _isWebViewInitialized = false;
        private bool _isSyncingFromWebView = false;

        public ApplicationFormWindow(ApplicationFormViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private async void DescriptionWebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                _isWebViewInitialized = true;

                // Subscribe to web messages from JavaScript
                DescriptionWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                UpdateWebViewContent();
            }
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // Update ViewModel when content is edited in the WebView
            _isSyncingFromWebView = true;
            try
            {
                ViewModel.Description = e.TryGetWebMessageAsString();
            }
            finally
            {
                _isSyncingFromWebView = false;
            }
        }

        private void UpdateWebViewContent()
        {
            // Skip update if the change originated from the WebView itself
            if (_isSyncingFromWebView)
                return;

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
        body:empty:before {{
            content: 'Paste or type the job description...';
            color: #999;
        }}
    </style>
</head>
<body contenteditable=""true"">
    {content}
</body>
<script>
    // Send content changes back to C#
    document.body.addEventListener('input', function() {{
        window.chrome.webview.postMessage(document.body.innerHTML);
    }});
</script>
</html>";
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Validate())
            {
                MessageBox.Show("Please fill in all required fields (Company and Role).",
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

            // Add initial status item only for new applications
            if (ViewModel.StatusItems.Count == 0)
            {
                ViewModel.AddStatusCommand.Execute(null);
            }
        }
    }
}
