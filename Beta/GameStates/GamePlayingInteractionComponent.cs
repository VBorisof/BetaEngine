using System;
using System.Linq;
using Beta.Cursors;
using Beta.DI;
using Beta.Entities;
using Beta.Scenes;
using Microsoft.Xna.Framework;

namespace Beta.GameStates;

public class GamePlayingInteractionComponent
{
    private readonly SceneManager _sceneManager;
    private readonly EntityManager _entityManager;

    public GamePlayingInteractionComponent()
    {
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();
    }

    public GameInteractionInputComponentResult QueryAtScenePos(Vector2 mouseScenePos)
    {
        var scene = _sceneManager.CurrentScene ??
            throw new InvalidOperationException("No scene defined.");

        var entities = _entityManager.GetOnScene(scene)
            .OrderBy(e => e.GetBoundingRect().Size.X * e.GetBoundingRect().Size.Y);

        // Try find entity
        foreach (var entity in entities)
        {
            if (entity.Contains(mouseScenePos))
            {
                return new GameInteractionInputComponentResult
                {
                    HoverSubject = CursorHoverSubject.Entity,
                    Entity = entity,
                };
            }
        }

        // Try find prop
        foreach (var prop in scene.Props)
        {
            if (prop.Polygon.Contains(mouseScenePos))
            {
                return new GameInteractionInputComponentResult
                {
                    HoverSubject = CursorHoverSubject.Prop,
                    Prop = prop,
                };
            }
        }

        // Try find exit
        foreach (var exit in scene.Exits)
        {
            if (exit.Polygon.Contains(mouseScenePos))
            {
                return new GameInteractionInputComponentResult
                {
                    HoverSubject = CursorHoverSubject.Exit,
                    Exit = exit,
                };
            }
        }

        // Fallback to something we walk on
        return new GameInteractionInputComponentResult
        {
            HoverSubject = CursorHoverSubject.WalkableArea,
            Move = mouseScenePos,
        };
    }
}