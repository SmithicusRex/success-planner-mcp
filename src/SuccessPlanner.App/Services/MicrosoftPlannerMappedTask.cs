using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed record MicrosoftPlannerMappedTask(
    TaskItem LocalTask,
    SourceLink SourceLink);
