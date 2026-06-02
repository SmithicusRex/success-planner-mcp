using Microsoft.Win32;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftProjectFilePicker : IMicrosoftProjectFilePicker
{
    public Task<string?> PickProjectFileAsync(
        string currentFilePath = "",
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        OpenFileDialog dialog = new()
        {
            Title = "Select Microsoft Project file",
            Filter = "Microsoft Project files (*.mpp)|*.mpp|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (!string.IsNullOrWhiteSpace(currentFilePath))
        {
            string directory = Path.GetDirectoryName(currentFilePath) ?? string.Empty;
            if (Directory.Exists(directory))
            {
                dialog.InitialDirectory = directory;
            }

            dialog.FileName = Path.GetFileName(currentFilePath);
        }

        bool? result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FileName : null);
    }
}
