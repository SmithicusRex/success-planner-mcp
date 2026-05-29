using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Screens;
using SuccessPlanner.App.Services;

namespace SuccessPlanner.App.ViewModels;

public sealed class AppShellViewModel : INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;
    private readonly AsyncRelayCommand _goBackCommand;
    private readonly AsyncRelayCommand _goHomeCommand;
    private IScreenViewModel _currentScreen;

    public AppShellViewModel(string statusText, string footerText, INavigationService navigationService)
    {
        _navigationService = navigationService;
        StatusText = statusText;
        FooterText = footerText;
        _currentScreen = navigationService.CurrentScreen
            ?? throw new InvalidOperationException("Navigation must have a current screen before the shell is created.");

        _goBackCommand = new AsyncRelayCommand(
            () => _navigationService.GoBackAsync(),
            () => CanGoBack);

        _goHomeCommand = new AsyncRelayCommand(
            () => _navigationService.GoHomeAsync(),
            () => !IsHome);

        navigationService.Navigated += OnNavigated;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusText { get; }

    public string FooterText { get; }

    public ICommand GoBackCommand => _goBackCommand;

    public ICommand GoHomeCommand => _goHomeCommand;

    public bool CanGoBack => _navigationService.CanGoBack;

    public bool IsHome => CurrentScreen.Descriptor.Screen == AppScreen.Home;

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
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(IsHome));
        _goBackCommand.RaiseCanExecuteChanged();
        _goHomeCommand.RaiseCanExecuteChanged();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
