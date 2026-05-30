using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class CaptureViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private const string EmptyTitleMessage = "Add one small action first.";
    private const string ReadyStatus = "Ready to capture.";

    private string _taskTitle = string.Empty;
    private string _notes = string.Empty;
    private string _validationMessage = string.Empty;
    private string _statusText = ReadyStatus;

    public CaptureViewModel()
        : base(ScreenCatalog.Capture)
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

    public string TaskTitle
    {
        get => _taskTitle;
        set
        {
            if (!SetProperty(ref _taskTitle, value))
            {
                return;
            }

            OnPropertyChanged(nameof(CanCreateTask));

            if (CanCreateTask)
            {
                ValidationMessage = string.Empty;
            }
        }
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool CanCreateTask => !string.IsNullOrWhiteSpace(TaskTitle);

    public bool TryCreateCapturedTask(out TaskItem? task)
    {
        if (!CanCreateTask)
        {
            task = null;
            ValidationMessage = EmptyTitleMessage;
            StatusText = "Capture needs a task title.";
            return false;
        }

        task = TaskItem.Capture(TaskTitle);
        task.UpdateNotes(Notes);
        ValidationMessage = string.Empty;
        StatusText = "Task ready to save.";
        return true;
    }

    public void ResetCaptureForm()
    {
        TaskTitle = string.Empty;
        Notes = string.Empty;
        ValidationMessage = string.Empty;
        StatusText = ReadyStatus;
    }

    private bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
