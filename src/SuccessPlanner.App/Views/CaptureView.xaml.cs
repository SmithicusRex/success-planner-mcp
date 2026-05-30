using System.Windows.Controls;

namespace SuccessPlanner.App.Views;

public partial class CaptureView : UserControl
{
    public CaptureView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TaskTitleTextBox.Focus();
    }
}
