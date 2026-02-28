using Microsoft.Xna.Framework;

namespace Beta.Dialogues;

public record DialogueOption
{
    public required int Index { get; init; }
    public required DialogueEdge Option { get; init; }
    public required Vector2 Position { get; init; }
}
