namespace SuccessPlanner.App.Services;

public sealed class PhoneCompanionCaptureImportOutcome
{
    private PhoneCompanionCaptureImportOutcome(
        string clientCaptureId,
        PhoneCompanionCaptureImportState state,
        string message,
        Guid? localTaskId = null)
    {
        ValidateState(state);

        ClientCaptureId = NormalizeRequired(clientCaptureId, nameof(clientCaptureId));
        State = state;
        Message = NormalizeRequired(message, nameof(message));
        LocalTaskId = localTaskId;
    }

    public string ClientCaptureId { get; }

    public PhoneCompanionCaptureImportState State { get; }

    public string Message { get; }

    public Guid? LocalTaskId { get; }

    public bool WasImported => State == PhoneCompanionCaptureImportState.Imported;

    public static PhoneCompanionCaptureImportOutcome Imported(
        string clientCaptureId,
        Guid localTaskId)
    {
        if (localTaskId == Guid.Empty)
        {
            throw new ArgumentException("Local task id cannot be empty.", nameof(localTaskId));
        }

        return new PhoneCompanionCaptureImportOutcome(
            clientCaptureId,
            PhoneCompanionCaptureImportState.Imported,
            "Imported into local inbox.",
            localTaskId);
    }

    public static PhoneCompanionCaptureImportOutcome Skipped(
        string clientCaptureId,
        string message)
    {
        return new PhoneCompanionCaptureImportOutcome(
            clientCaptureId,
            PhoneCompanionCaptureImportState.Skipped,
            message);
    }

    public static PhoneCompanionCaptureImportOutcome Rejected(
        string clientCaptureId,
        string message)
    {
        return new PhoneCompanionCaptureImportOutcome(
            clientCaptureId,
            PhoneCompanionCaptureImportState.Rejected,
            message);
    }

    private static void ValidateState(PhoneCompanionCaptureImportState state)
    {
        if (!Enum.IsDefined(typeof(PhoneCompanionCaptureImportState), state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), "Phone capture import state is not valid.");
        }
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be blank.", parameterName);
        }

        return value.Trim();
    }
}
