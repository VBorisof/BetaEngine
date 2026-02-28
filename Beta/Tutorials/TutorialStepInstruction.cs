using Beta.Extensions.Models;
using System.Text.Json.Serialization;

namespace Beta.Tutorials;

public record TutorialStepInstruction
{
    [JsonPropertyName("instruction")]
    public required string Instruction { get; init; }
    [JsonPropertyName("position")]
    public required Vector2Model Position { get; init; }
    [JsonPropertyName("duration")]
    public required int? Duration { get; init; }
    [JsonPropertyName("highlight")]
    public Vector2Model? Highlight { get; init; }
    [JsonPropertyName("highlightRadius")]
    public float? HighlightRadius { get; init; }
}