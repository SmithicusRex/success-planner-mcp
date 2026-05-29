using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class InitialScreenViewModel : ScreenViewModelBase
{
    public InitialScreenViewModel(AppScreenDescriptor descriptor)
        : base(descriptor)
    {
    }

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;
}
