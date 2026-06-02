namespace SuccessPlanner.App.Services;

public interface IMicrosoftProjectFilePicker
{
    Task<string?> PickProjectFileAsync(
        string currentFilePath = "",
        CancellationToken cancellationToken = default);
}
