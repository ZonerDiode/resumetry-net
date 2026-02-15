using System.Windows;
using System.Windows.Controls;
using Resumetry.ViewModels;
using Microsoft.Web.WebView2.Core;

namespace Resumetry.Views
{
    /// <summary>
    /// Interaction logic for ApplicationFormView.xaml
    /// </summary>
    public partial class ApplicationFormView : UserControl
    {
        private ApplicationFormViewModel? ViewModel => DataContext as ApplicationFormViewModel;
        private bool _isWebViewInitialized = false;
        private bool _isSyncingFromWebView = false;

        public ApplicationFormView()
        {
            InitializeComponent();
        }

        private async void ApplicationFormView_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Initialize WebView2
            await DescriptionWebView.EnsureCoreWebView2Async(null);

            // Add initial status item only for new applications
            if (ViewModel != null && ViewModel.ApplicationStatuses.Count == 0)
            {
                ViewModel.AddStatusCommand.Execute(null);
            }
        }

        private void ApplicationFormView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Cleanup WebView2 to prevent memory leaks
            if (DescriptionWebView.CoreWebView2 != null)
            {
                DescriptionWebView.CoreWebView2.WebMessageReceived -= CoreWebView2_WebMessageReceived;
                DescriptionWebView.Dispose();
            }

            _isWebViewInitialized = false;
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
                if (ViewModel != null)
                {
                    ViewModel.Description = e.TryGetWebMessageAsString();
                }
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

            if (_isWebViewInitialized && DescriptionWebView.CoreWebView2 != null && ViewModel != null)
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
    }
}
