using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Screens;
using SuccessPlanner.App.Services;

namespace SuccessPlanner.App.ViewModels;

public sealed class AppShellViewModel : INotifyPropertyChanged
{
    private IScreenViewModel _currentScreen;

    public AppShellViewModel(string statusText, string footerText, INavigationService navigationService)
    {
        StatusText = statusText;
        FooterText = footerText;
        _currentScreen = navigationService.CurrentScreen
            ?? throw new InvalidOperationException("Navigation must have a current screen before the shell is created.");

        navigationService.Navigated += OnNavigated;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusText { get; }

    public string FooterText { get; }

    public IScreenViewModel CurrentScreen
    {
        get => _currentScreen;
        private set
        {
            if (ReferenceEquals(_currentScreen, value))
            {
                return;
            }

            _currentScreen = value;
            OnPropertyChanged();
        }
    }

    private void OnNavigated(object? sender, NavigationChangedEventArgs e)
    {
        CurrentScreen = e.CurrentScreen;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
