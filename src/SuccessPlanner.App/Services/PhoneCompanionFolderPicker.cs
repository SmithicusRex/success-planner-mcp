using Microsoft.Win32;

namespace SuccessPlanner.App.Services;

public sealed class PhoneCompanionFolderPicker : IPhoneCompanionFolderPicker
{
    public Task<string?> PickCaptureFolderAsync(
        string currentFolderPath = "",
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        OpenFolderDialog dialog = new()
        {
            Title = "Select Phone Companion capture folder",
            Multiselect = false
        };

        if (!string.IsNullOrWhiteSpace(currentFolderPath)
            && Directory.Exists(currentFolderPath))
        {
            dialog.InitialDirectory = currentFolderPath;
            dialog.FolderName = currentFolderPath;
        }

        bool? result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FolderName : null);
    }
}
