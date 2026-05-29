using System.Windows;
using SuccessPlanner.App.Bootstrap;

namespace SuccessPlanner.App;

public partial class App : Application
{
    private AppBootstrapper? _bootstrapper;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        _bootstrapper = new AppBootstrapper();

        BootstrapResult result = await _bootstrapper.StartAsync();
        if (!result.Success)
        {
            MessageBox.Show(
                result.UserMessage,
                "Success Planner MCP",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
            return;
        }

        MainWindow = result.MainWindow;
        if (MainWindow is null)
        {
            Shutdown(1);
            return;
        }

        MainWindow.Closed += OnMainWindowClosed;
        MainWindow.Show();
    }

    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
        Shutdown();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        if (_bootstrapper is not null)
        {
            _bootstrapper.StopAsync().GetAwaiter().GetResult();
        }
    }
}
