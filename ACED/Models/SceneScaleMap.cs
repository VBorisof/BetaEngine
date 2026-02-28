using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended.Shapes;
using System.Text.Json.Serialization;

namespace aced.Models;

public class SceneScaleMap : ISceneNodeList
{
    [JsonPropertyName("maxPivot")]
    public SceneNode MaxPivot { get; set; }
    [JsonPropertyName("minPivot")]
    public SceneNode MinPivot { get; set; }
    [JsonPropertyName("nodes")]
    public List<SceneNode> Nodes { get; set; } = [];

    [JsonPropertyName("maxScale")]
    public float MaxScale { get; set; } = 1f;
    [JsonPropertyName("minScale")]
    public float MinScale { get; set; } = 0.01f;

    [JsonIgnore]
    public bool IsMapValid { get; private set; } = false;
    [JsonIgnore]
    public Dictionary<int, Dictionary<int, float>> Map { get; } = [];

    public void BuildMap()
    {
        if (MaxPivot == null || MinPivot == null)
        {
            return;
        }

        var sortedNodes = Nodes.OrderBy(n => n.Id);
        var poly = new Polygon(sortedNodes.Select(n => new Vector2(n.X, n.Y)));
        var minX = sortedNodes.Min(n => n.X);
        var maxX = sortedNodes.Max(n => n.X);
        var minY = sortedNodes.Min(n => n.Y);
        var maxY = sortedNodes.Max(n => n.Y);

        var distance = 1;
        for (var y = minY; y < maxY; y += distance)
        {
            for (var x = minX; x < maxX; x += distance)
            {
                var xy = new Vector2(x, y);
                if (poly.Contains(xy))
                {
                    float x1 = MaxPivot.X;
                    float y1 = MaxPivot.Y;
                    float x2 = MinPivot.X;
                    float y2 = MinPivot.Y;

                    var dxC = x - x1;
                    var dyC = y - y1;

                    var offset = new Vector2(0, dyC).Length() / (new Vector2(0, y2) - new Vector2(0, y1)).Length();

                    var alpha = MaxScale + ((MinScale - MaxScale) * offset);

                    if (!Map.TryGetValue(y, out var value))
                    {
                        value = [];
                        Map[y] = value;
                    }

                    value[x] = alpha;

                    IsMapValid = true;
                }
            }
        }
    }

    public void ClearMap()
    {
        Map.Clear();
        IsMapValid = false;
    }

    public float GetScale(Vector2 pos)
    {
        if (!IsMapValid)
        {
            return 1f;
        }

        var x = (int)pos.X;
        var y = (int)pos.Y;

        if (Map.ContainsKey(y) && Map[y].ContainsKey(x))
        {
            return Map[y][x];
        }

        return 1f;
    }

    public void ExportToPng(int width, int height, GraphicsDevice graphics, string filePath)
    {
        if (!IsMapValid)
        {
            return;
        }

        var texture = new Texture2D(graphics, width, height);

        var size = width * height;
        var pixels = new Color[size];
        foreach (var yValue in Map)
        {
            if (yValue.Key < 0 || yValue.Key >= height)
            {
                continue;
            }

            foreach (var xValue in yValue.Value)
            {
                if (xValue.Key < 0 || xValue.Key >= width)
                {
                    continue;
                }

                int index = ((yValue.Key - 1) * width) + xValue.Key;
                if (index < 0 || index >= size)
                {
                    continue;
                }
                pixels[index] = Color.Yellow * xValue.Value;
            }
        }
        texture.SetData(pixels);

        using var stream = new FileStream(filePath, FileMode.Create);
        texture.SaveAsPng(stream, width, height);
    }
}