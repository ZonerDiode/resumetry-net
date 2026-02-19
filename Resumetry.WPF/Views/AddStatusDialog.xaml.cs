using System.Windows;
using Resumetry.ViewModels;

namespace Resumetry.Views;

public partial class AddStatusDialog : Window
{
    public AddStatusDialog(AddStatusDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += confirmed => DialogResult = confirmed;
    }
}
