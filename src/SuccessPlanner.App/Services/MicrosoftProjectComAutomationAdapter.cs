using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftProjectComAutomationAdapter : IMicrosoftProjectAutomationAdapter
{
    public async Task<IReadOnlyList<MicrosoftProjectImportedTask>> ImportTasksAsync(
        string projectFilePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectFilePath))
        {
            throw new ArgumentException("Project file path cannot be blank.", nameof(projectFilePath));
        }

        TaskCompletionSource<IReadOnlyList<MicrosoftProjectImportedTask>> completion = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using CancellationTokenRegistration registration =
            cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));

        Thread thread = new(() =>
        {
            try
            {
                completion.TrySetResult(ImportTasksOnCurrentThread(projectFilePath, cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                completion.TrySetCanceled(cancellationToken);
            }
            catch (Exception ex)
            {
                completion.TrySetException(ex);
            }
        })
        {
            IsBackground = true,
            Name = "Microsoft Project Import"
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return await completion.Task.ConfigureAwait(false);
    }

    private static IReadOnlyList<MicrosoftProjectImportedTask> ImportTasksOnCurrentThread(
        string projectFilePath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Type? projectType = Type.GetTypeFromProgID("MSProject.Application");
        if (projectType is null)
        {
            throw new InvalidOperationException("Microsoft Project desktop automation is not available.");
        }

        object? application = null;

        try
        {
            application = Activator.CreateInstance(projectType)
                ?? throw new InvalidOperationException("Microsoft Project desktop automation could not start.");

            dynamic projectApplication = application;
            TrySetProperty(application, "Visible", false);
            projectApplication.FileOpen(projectFilePath, true);

            object? activeProject = TryGetProperty(application, "ActiveProject");
            object? taskCollection = TryGetProperty(activeProject, "Tasks");

            List<MicrosoftProjectImportedTask> importedTasks = [];
            foreach (object taskObject in EnumerateComCollection(taskCollection))
            {
                cancellationToken.ThrowIfCancellationRequested();

                MicrosoftProjectImportedTask? importedTask = ReadProjectTask(taskObject);
                if (importedTask is not null)
                {
                    importedTasks.Add(importedTask);
                }
            }

            TryInvoke(application, "FileCloseEx", 0);
            return importedTasks;
        }
        finally
        {
            TryInvoke(application, "Quit", 0);
            ReleaseComObject(application);
        }
    }

    private static MicrosoftProjectImportedTask? ReadProjectTask(object taskObject)
    {
        string name = ReadString(taskObject, "Name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        string externalId = ReadString(taskObject, "UniqueID");
        if (string.IsNullOrWhiteSpace(externalId))
        {
            externalId = ReadString(taskObject, "ID");
        }

        return new MicrosoftProjectImportedTask(
            externalId,
            name,
            ReadDate(taskObject, "Start"),
            ReadDate(taskObject, "Finish"),
            ReadInt(taskObject, "PercentComplete"),
            ReadString(taskObject, "Notes"),
            ReadInt(taskObject, "Duration"),
            ReadInt(taskObject, "OutlineLevel"),
            ReadBoolean(taskObject, "Summary"),
            ReadBoolean(taskObject, "Milestone"),
            ReadBoolean(taskObject, "Critical"),
            ReadInt(taskObject, "Priority"));
    }

    private static IEnumerable<object> EnumerateComCollection(object? collection)
    {
        if (collection is not IEnumerable enumerable)
        {
            yield break;
        }

        foreach (object? item in enumerable)
        {
            if (item is not null)
            {
                yield return item;
            }
        }
    }

    private static string ReadString(object target, string propertyName)
    {
        return TryGetProperty(target, propertyName)?.ToString()?.Trim() ?? string.Empty;
    }

    private static DateTimeOffset? ReadDate(object target, string propertyName)
    {
        object? value = TryGetProperty(target, propertyName);
        if (value is null)
        {
            return null;
        }

        if (value is DateTime dateTime)
        {
            return dateTime.Year <= 1900
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Local));
        }

        if (value is double oaDate)
        {
            DateTime oaDateTime = DateTime.FromOADate(oaDate);
            return oaDateTime.Year <= 1900
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(oaDateTime, DateTimeKind.Local));
        }

        string text = value.ToString()?.Trim() ?? string.Empty;
        return DateTimeOffset.TryParse(
            text,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out DateTimeOffset parsed)
            ? parsed
            : null;
    }

    private static int? ReadInt(object target, string propertyName)
    {
        object? value = TryGetProperty(target, propertyName);
        if (value is null)
        {
            return null;
        }

        try
        {
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (InvalidCastException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    private static bool ReadBoolean(object target, string propertyName)
    {
        object? value = TryGetProperty(target, propertyName);
        if (value is null)
        {
            return false;
        }

        if (value is bool boolean)
        {
            return boolean;
        }

        string text = value.ToString()?.Trim() ?? string.Empty;
        return string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(text, "1", StringComparison.OrdinalIgnoreCase);
    }

    private static object? TryGetProperty(object? target, string propertyName)
    {
        if (target is null)
        {
            return null;
        }

        try
        {
            return target.GetType().InvokeMember(
                propertyName,
                BindingFlags.GetProperty,
                binder: null,
                target,
                args: null,
                CultureInfo.InvariantCulture);
        }
        catch (COMException)
        {
            return null;
        }
        catch (MissingMethodException)
        {
            return null;
        }
        catch (TargetInvocationException)
        {
            return null;
        }
    }

    private static void TrySetProperty(object? target, string propertyName, object value)
    {
        if (target is null)
        {
            return;
        }

        try
        {
            target.GetType().InvokeMember(
                propertyName,
                BindingFlags.SetProperty,
                binder: null,
                target,
                args: [value],
                CultureInfo.InvariantCulture);
        }
        catch (COMException)
        {
        }
        catch (MissingMethodException)
        {
        }
        catch (TargetInvocationException)
        {
        }
    }

    private static void TryInvoke(object? target, string methodName, params object[] args)
    {
        if (target is null)
        {
            return;
        }

        try
        {
            target.GetType().InvokeMember(
                methodName,
                BindingFlags.InvokeMethod,
                binder: null,
                target,
                args,
                CultureInfo.InvariantCulture);
        }
        catch (COMException)
        {
        }
        catch (MissingMethodException)
        {
        }
        catch (TargetInvocationException)
        {
        }
    }

    private static void ReleaseComObject(object? comObject)
    {
        if (comObject is not null && Marshal.IsComObject(comObject))
        {
            Marshal.FinalReleaseComObject(comObject);
        }
    }
}
