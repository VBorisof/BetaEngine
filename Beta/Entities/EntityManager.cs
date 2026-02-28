using Beta.Actors;
using Beta.Scenes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Beta.Entities;

public class EntityManager
{
    public Actor? Player { get; set; }
    public Dictionary<string, Entity> Entities { get; } = [];

    public T Get<T>(string name) where T : Entity
    {
        return (T)Entities[name];
    }
    public bool Contains(string name)
    {
        return Entities.ContainsKey(name);
    }

    public void Add(Entity e)
    {
        Entities[e.DeclName] = e;
    }

    public IEnumerable<Entity> GetOnScene(Scene scene)
    {
        return Entities.Select(e => e.Value).Where(e => e.Scene == scene);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var entity in Entities)
        {
            entity.Value.Update(gameTime);
        }
    }

    public void RemoveAllFromScenes()
    {
        foreach (var entity in Entities)
        {
            entity.Value.Scene = null;
        }
    }
}