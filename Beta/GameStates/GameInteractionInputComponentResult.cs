using Beta.Cursors;
using Beta.Entities;
using Beta.Scenes;
using Microsoft.Xna.Framework;

namespace Beta.GameStates;

public record GameInteractionInputComponentResult
{
    public required CursorHoverSubject HoverSubject { get; init; }
    public Entity? Entity { get; set; }
    public SceneProp? Prop { get; set; }
    public Vector2? Move { get; set; }
    public SceneExit? Exit { get; set; }
}