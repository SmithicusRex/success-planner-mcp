namespace SuccessPlanner.App.Services;

public enum LocalSearchResultKind
{
    Task,
    Note,
    Project,
    SourceLink
}

public sealed record LocalSearchResult(
    LocalSearchResultKind Kind,
    Guid ItemId,
    string Title,
    string Detail,
    string SourceText,
    DateTimeOffset CreatedAt,
    string LocalItemType = "",
    Guid? LocalItemId = null,
    string ExternalWebUrl = "");
