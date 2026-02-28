using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace Beta.Tutorials;

public class TutorialStep
{
    [JsonPropertyName("stepStyle")]
    [JsonConverter(typeof(JsonStringEnumConverter<TutorialStepStyle>))]
    public required TutorialStepStyle StepStyle { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("actionToMatch")]
    public TutorialStepAction? ActionToMatch { get; init; }

    [JsonPropertyName("allowedActions")]
    public TutorialStepAction[] AllowedActions { get; init; } = [];

    [JsonPropertyName("waitMsOnMatch")]
    public int? WaitMsOnMatch { get; init; }


    public void Draw(SpriteBatch spriteBatch)
    {
        /*
        if (Instruction.Highlight is not null && Instruction.HighlightRadius is not null)
        {
            spriteBatch.DrawCircle(
                Instruction.Highlight.ToVector2(),
                Instruction.HighlightRadius.Value,
                64,
                Color.Red,
                thickness: 5,
                layerDepth: Constants.LayerDepthPopup
            );
        }
        */
    }
}