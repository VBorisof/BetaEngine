using System.Collections.Generic;
using Beta.Entities.Animations;

namespace Beta.Entities.Costumes;

public class Costume
{
    public string Name { get; set; } = string.Empty;
    public List<Animation> Animations { get; set; } = [];
}