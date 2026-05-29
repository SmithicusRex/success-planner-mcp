namespace SuccessPlanner.App.Screens;

public static class ScreenCatalog
{
    public static AppScreenDescriptor Home { get; } = new(
        AppScreen.Home,
        "Home",
        "Choose the next simple action.",
        "\uE80F",
        "#2F6FED");

    public static AppScreenDescriptor Capture { get; } = new(
        AppScreen.Capture,
        "Capture",
        "Add the thought before it escapes.",
        "\uE710",
        "#9DCCFF");

    public static AppScreenDescriptor Today { get; } = new(
        AppScreen.Today,
        "Today",
        "See what matters now.",
        "\uE787",
        "#A8E6B1");

    public static AppScreenDescriptor Plan { get; } = new(
        AppScreen.Plan,
        "Plan",
        "Make the next step smaller.",
        "\uE9D5",
        "#FFE08A");

    public static AppScreenDescriptor StartWork { get; } = new(
        AppScreen.StartWork,
        "Start",
        "Begin a short focus session.",
        "\uE768",
        "#8DDAD5");

    public static AppScreenDescriptor Done { get; } = new(
        AppScreen.Done,
        "Done",
        "Record the win.",
        "\uE73E",
        "#DADDE2");

    public static AppScreenDescriptor Move { get; } = new(
        AppScreen.Move,
        "Move",
        "Walk, stretch, or work out.",
        "\uE805",
        "#FFBE7A");

    public static AppScreenDescriptor Review { get; } = new(
        AppScreen.Review,
        "Review",
        "Notice progress and choose what matters next.",
        "\uE9D2",
        "#C8B6FF");

    public static AppScreenDescriptor Find { get; } = new(
        AppScreen.Find,
        "Find",
        "Search local tasks and notes.",
        "\uE721",
        "#FFFFFF");

    public static AppScreenDescriptor Settings { get; } = new(
        AppScreen.Settings,
        "Settings",
        "Adjust the personal control center.",
        "\uE713",
        "#FFFFFF");

    public static IReadOnlyList<AppScreenDescriptor> All { get; } =
    [
        Home,
        Capture,
        Today,
        Plan,
        StartWork,
        Done,
        Move,
        Review,
        Find,
        Settings
    ];

    public static AppScreenDescriptor Get(AppScreen screen)
    {
        return All.First(descriptor => descriptor.Screen == screen);
    }
}
