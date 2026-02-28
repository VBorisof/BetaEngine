using Beta.Logging;
using Beta.Text;
using Microsoft.Xna.Framework.Audio;

namespace Beta.Gui;

public record GuiConfiguration
{
    public required string LayoutFile { get; init; }
    public required string StyleFile { get; init; }
    public required FontBinding GuiFontBinding { get; init; }
    public required float BaseLayerDepth { get; init; }
    public LogLevel LogLevel { get; init; } = LogLevel.Info;

    public SoundEffect? HoverUiElemSound { get; set; }
    public SoundEffect? ClickUiElemSound { get; set; }
    public SoundEffect? NotifySound { get; set; }

    // TODO: Screen size?
}