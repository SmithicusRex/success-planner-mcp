using SuccessPlanner.App.Infrastructure;

namespace SuccessPlanner.App.ViewModels;

public sealed class DestinationRuleSummaryViewModel
{
    public DestinationRuleSummaryViewModel(DestinationRuleSettings settings)
    {
        Name = settings.Name;
        Condition = settings.Condition;
        Destination = $"{settings.DestinationSystem}: {settings.DestinationName}";
    }

    public string Name { get; }

    public string Condition { get; }

    public string Destination { get; }
}
