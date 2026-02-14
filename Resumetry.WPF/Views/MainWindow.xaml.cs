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
        public MainWindow(ShellViewModel viewModel)
        {
            InitializeComponent();
            UpdateMaximizeGlyph();
            DataContext = viewModel;
        }

        // Make title bar draggable and support double-click to maximize/restore
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximizeRestore();
                return;
            }

            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // Ignore if DragMove is not valid in current state
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaxRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximizeRestore();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleMaximizeRestore()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }

            UpdateMaximizeGlyph();
        }

        private void UpdateMaximizeGlyph()
        {
            if (MaxRestoreGlyph == null)
                return;

            MaxRestoreGlyph.Text = (WindowState == WindowState.Maximized) ? "❐" : "▢";
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            UpdateMaximizeGlyph();
        }
    }
}