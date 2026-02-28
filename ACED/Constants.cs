namespace aced;

public static class Constants
{
    public static float BaseLayerDepth { get; } = 0.8f;
    public static float LayerDepthStep { get; } = 0.0000001f;
    public static float LayerDepthMacroStep { get; } = 0.0001f;

    public static float LayerDepthViewport { get; } = 0.1f;
    public static float LayerDepthScene { get; } = 0.20f;
    public static float LayerDepthScaleMap { get; } = 0.205f;
    public static float LayerDepthActor { get; } = 0.21f;
    public static float LayerDepthWireframe { get; } = 0.22f;
    public static float LayerDepthLight { get; } = 0.23f;
    public static float LayerDepthEdge { get; } = 0.30f;
    public static float LayerDepthNode { get; } = 0.31f;
}