using Beta.Common.Extensions;
using Microsoft.Xna.Framework;

namespace Beta;

public static class Constants
{
    public static float LayerDepthScene { get; } = 0.0001f;
    public static float LayerDepthWbBase { get; } = 0.001f;
    public static float LayerDepthEntity { get; } = 0.1f;
    public static float LayerDepthCloseup { get; } = 0.2f;
    public static float LayerDepthDebug { get; } = 0.6f;
    public static float LayerDepthSpeech { get; } = 0.7f;
    public static float LayerDepthVideo { get; } = 0.74f;
    public static float LayerDepthFade { get; } = 0.75f;
    public static float LayerDepthGui { get; } = 0.8f;
    public static float LayerDepthPopup { get; } = 0.85f;
    public static float LayerDepthText { get; } = 0.9f;
    public static float LayerDepthCursor { get; } = 0.95f;
    public static float LayerDepthStep { get; } = 0.0001f;
    public static float LayerDepthMicroStep { get; } = 0.000001f;

    public static int TextWidthActorSpeech { get; } = 20;
    public static int TextWidthNarration { get; } = 80;

    // TODO: Remove this... Maybe hook up in GuiTheme somehow.
    public static Color MainColor { get; } = ColorEx.FromHexString("628395AA");
    public static Color MainTextColor { get; } = ColorEx.FromHexString("E6E8E6");
    public static Color MainTextOutlineColor { get; } = ColorEx.FromHexString("212738");
    public static Color MainTextHighlightColor { get; } = ColorEx.FromHexString("DF2935");

    public static int DefaultAdapterWidth { get; } = 1920;
    public static int DefaultAdapterHeight { get; } = 1080;
}