using Microsoft.Xna.Framework;

namespace Beta.Scenes.Pathfinding;

public record GraphNode
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public GraphNode? Parent { get; set; }

    public float F { get; set; } = 1000;
    public float G { get; set; } = 1000;

    public GraphNode(Vector2 position)
    {
        Position = position;
    }

    public GraphNode(GraphNode other)
    {
        Position = other.Position;
    }
}