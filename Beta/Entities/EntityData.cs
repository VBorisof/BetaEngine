using System.Collections.Generic;
using Beta.Entities.Costumes;
using Microsoft.Xna.Framework;

namespace Beta.Entities;

public class EntityData
{
    public List<Costume> Costumes { get; set; } = [];
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public float Speed { get; set; } = 1.0f;
}