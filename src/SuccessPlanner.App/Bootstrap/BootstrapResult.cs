using System.Windows;

namespace SuccessPlanner.App.Bootstrap;

public sealed class BootstrapResult
{
    private BootstrapResult(bool success, string userMessage, Window? mainWindow)
    {
        Success = success;
        UserMessage = userMessage;
        MainWindow = mainWindow;
    }

    public bool Success { get; }

    public string UserMessage { get; }

    public Window? MainWindow { get; }

    public static BootstrapResult Ready(Window mainWindow)
    {
        return new BootstrapResult(true, "Ready", mainWindow);
    }

    public static BootstrapResult Failed(string userMessage)
    {
        return new BootstrapResult(false, userMessage, null);
    }
}
