using Beta.DI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Linq;
using Beta.Logging;
using Beta.Extensions;
using System.Collections.Generic;
using Beta.Actors;
using Beta.Entities;
using Beta.Services.Sounds;

namespace Beta.Scenes;

public class SceneManager
{
    private readonly ILogger _logger;
    private readonly OrthographicCamera _camera;
    private readonly EntityManager _entityManager;

    private readonly MusicPlayerService _musicPlayer;

    public Dictionary<string, Scene> Scenes { get; } = [];

    public Scene? CurrentScene { get; set; }

    public SceneManager()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();

        _entityManager = DependencyContainer.Instance.Get<EntityManager>();

        _musicPlayer = DependencyContainer.Instance.Get<MusicPlayerService>();
    }


    /*
     * TODO: Encapsulate pathfinding here?
     * */

    public void AddScene(Scene scene)
    {
        Scenes[scene.Name] = scene;
    }

    public Scene GetScene(string name)
    {
        return Scenes[name];
    }

    // Cases for scene switching:
    // 1. Player requests a scene transition via exit.
    //   -> ExitScene
    //     - Assume we have an exit
    //     - Assume we have a player.
    // 2. Script requests scene setting.
    //   -> SetScene
    // 3. We load the game and set the scene.
    //   -> SetScene

    public void ExitScene(Actor player, SceneExit startExit)
    {
        _logger.Debug();

        // Which scene do we want to go to?
        var newScene = Scenes[startExit.Destination];

        // Where do we want to be place the player?
        var exit = newScene.Exits.Single(e => e.StartIndex == startExit.TargetIndex);
        player.Position = new Vector2(exit.ExitPoint.X, exit.ExitPoint.Y);
        player.Scene = newScene;
        player.Region = null;

        AdjustCamera(newScene);

        // Populate scene with actors, for scale adjustments etc.
        // Need to take care about entity states
        foreach (var metaActor in newScene.ScenePlacements)
        {
            var entity = _entityManager.Get<Entity>(metaActor.Name);
            if (entity == null)
            {
                _logger.Error($"Failed to get entity {metaActor.Name}");
                continue;
            }

            entity.NativeSceneScale = metaActor.Scale;

            if ((entity as Actor) == _entityManager.Player)
            {
                continue;
            }

            // Add overrides for this in case someone moves?
            // entity.Scene = newScene;
            // entity.Position = new Vector2(metaActor.Position.X, metaActor.Position.Y);
        }

        CurrentScene = newScene;

        SetEffects(newScene);

        // Music
        _musicPlayer.Play(CurrentScene.MusicName, MusicType.Scene);
    }

    public void SetScene(string sceneName)
    {
        _logger.Debug();

        // Which scene do we want to set?
        var newScene = Scenes[sceneName];

        AdjustCamera(newScene);

        foreach (var metaActor in newScene.ScenePlacements)
        {
            var entity = _entityManager.Get<Entity>(metaActor.Name);
            if (entity == null)
            {
                _logger.Error($"Failed to get entity {metaActor.Name}");
                continue;
            }

            entity.NativeSceneScale = metaActor.Scale;
            // entity.Scene = newScene;
            // entity.Position = new Vector2(metaActor.Position.X, metaActor.Position.Y);
        }


        CurrentScene = newScene;

        SetEffects(newScene);

        // Music
        _musicPlayer.Play(CurrentScene.MusicName, MusicType.Scene);
    }

    public void SetSceneNoEntityReset(string sceneName)
    {
        _logger.Debug();

        // Which scene do we want to set?
        var newScene = Scenes[sceneName];

        AdjustCamera(newScene);

        foreach (var metaActor in newScene.ScenePlacements)
        {
            var entity = _entityManager.Get<Entity>(metaActor.Name);
            if (entity == null)
            {
                _logger.Error($"Failed to get entity {metaActor.Name}");
                continue;
            }

            entity.NativeSceneScale = metaActor.Scale;
        }

        CurrentScene = newScene;

        SetEffects(newScene);

        // Music
        _musicPlayer.Play(CurrentScene.MusicName, MusicType.Scene);
    }

    public void NullScene()
    {
        CurrentScene = null;
    }

    public void Update(GameTime gameTime)
    {
        if (CurrentScene != null)
        {
            CurrentScene.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (CurrentScene != null)
        {
            CurrentScene.Draw(spriteBatch);
            if (Settings.IsDebug)
            {
                DrawDebugInfo(spriteBatch);
            }
        }
    }

    private void DrawDebugInfo(SpriteBatch spriteBatch)
    {
        if (CurrentScene is null)
        {
            return;
        }

        foreach (var exit in CurrentScene.Exits)
        {
            spriteBatch.DrawPolygon(
                new Vector2(0, 0),
                exit.Polygon,
                Color.Green,
                thickness: 2f,
                layerDepth: Constants.LayerDepthDebug
            );
        }
        foreach (var area in CurrentScene.WalkableAreas)
        {
            spriteBatch.DrawPolygon(
                new Vector2(0, 0),
                area.Polygon,
                Color.Red,
                thickness: 2f,
                layerDepth: Constants.LayerDepthDebug
            );
        }
        foreach (var node in CurrentScene.WalkGraph.Nodes)
        {
            spriteBatch.DrawPoint(node.Position, Color.Yellow, 10, layerDepth: Constants.LayerDepthScene + Constants.LayerDepthStep);
        }
        foreach (var edge in CurrentScene.WalkGraph.Edges.SelectMany(e => e))
        {
            var fromPos = CurrentScene.WalkGraph.Nodes[edge.From].Position;
            var toPos = CurrentScene.WalkGraph.Nodes[edge.To].Position;
            spriteBatch.DrawLine(
                fromPos,
                toPos,
                Color.Blue,
                4,
                layerDepth: Constants.LayerDepthScene + (Constants.LayerDepthStep * 2));
        }
    }

    private void SetEffects(Scene newScene)
    {
        newScene.SetEffects();
    }

    public void UpdateCurrentSceneEffects()
    {
        CurrentScene?.UpdateEffects();
    }

    private void AdjustCamera(Scene newScene)
    {
        // Handle the camera : Center, Clamping, look at the player, etc.
        if (newScene.Texture.Width < _camera.BoundingRectangle.Width)
        {
            _camera.Position = new Vector2(
                -(
                    (_camera.BoundingRectangle.Width / 2) - (newScene.Texture.Width / 2)
                ),
                0
            );
        }
        else
        {
            if (_entityManager.Player != null)
            {
                _camera.LookAt(_entityManager.Player.Position);
                _camera.ClampCameraX(0, newScene.Texture.Width - _camera.BoundingRectangle.Width);
                _camera.ClampCameraY(0, newScene.Texture.Height - _camera.BoundingRectangle.Height);
            }
        }
    }
}