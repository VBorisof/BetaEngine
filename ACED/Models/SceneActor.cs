using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Beta.Entities.Animations;
using System.Text.Json.Serialization;

namespace aced.Models;

public class SceneActorCostume
{
    public string Name { get; set; } = string.Empty;
    public List<Animation> Animations { get; set; } = [];
}

public class SceneActor
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("position")]
    public Coord Position { get; set; } = new Coord(0, 0);
    [JsonPropertyName("scale")]
    public float Scale { get; set; }
    [JsonPropertyName("isShowChildren")]
    public bool IsShowChildren { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; } = "";
    [JsonPropertyName("children")]
    public List<SceneActor> Children { get; set; } = [];

    [JsonIgnore]
    public List<SceneActorCostume> Costumes { get; set; } = [];
    [JsonIgnore]
    public SceneActorCostume CurrentCostume { get; set; }
    [JsonIgnore]
    public Animation CurrentAnimation { get; set; }
    [JsonIgnore]
    public ActorData Actor { get; set; }
    [JsonIgnore]
    public SceneActor Parent { get; set; }

    public Rectangle GetBoundingRect(Vector2 scenePos, float camZoom, float scaleMapScale)
    {
        if (CurrentAnimation == null)
        {
            return Rectangle.Empty;
        }
        var frame = CurrentAnimation.GetCurrentFrame();

        var scaleFactor = Scale * scaleMapScale / camZoom;
        var destinationRectangle = new Rectangle(
            (int)scenePos.X - (int)(frame.Width * Actor.Origin.X * scaleFactor),
            (int)scenePos.Y - (int)(frame.Height * Actor.Origin.Y * scaleFactor),
            (int)(frame.Width * scaleFactor),
            (int)(frame.Height * scaleFactor)
        );

        return destinationRectangle;
    }
}