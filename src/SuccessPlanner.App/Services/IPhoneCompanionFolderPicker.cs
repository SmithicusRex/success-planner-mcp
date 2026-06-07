namespace SuccessPlanner.App.Services;

public interface IPhoneCompanionFolderPicker
{
    Task<string?> PickCaptureFolderAsync(
        string currentFolderPath = "",
        CancellationToken cancellationToken = default);
}
