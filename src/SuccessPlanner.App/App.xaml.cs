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

        result.MainWindow?.Show();
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        if (_bootstrapper is not null)
        {
            await _bootstrapper.StopAsync();
        }
    }
}
