
namespace Blade.Tools.Windit;
[Export(typeof(GuiToolGroup))]
[Name("Blade")]
internal sealed class WinditGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal WinditGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0132';
        DisplayTitle = Windit.DisplayTitle;
        AccessibleName = Windit.AccessibleName;
    }
}
