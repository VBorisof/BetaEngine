using Beta.Actors;
using Microsoft.Xna.Framework;
using Beta.Entities;
using Beta.DI;
using Beta.Scenes;
using Beta.Logging;

namespace Beta.Commands;

public class ScenePutCommand : Command
{
    private readonly ILogger _logger;
    private readonly SceneManager _sceneManager;

    public Actor Who { get; }
    public Entity What { get; }
    public Vector2 Position { get; }

    public ScenePutCommand(Actor who, Entity what, Vector2 position)
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        Who = who;
        What = what;
        Position = position;
    }

    public override void Startup()
    {
        What.Position = Position;
        What.Scene = _sceneManager.CurrentScene;
        IsDone = true;
    }
    public override bool Update(GameTime gameTime)
    {
        return true;
    }

    public override void OnComplete()
    {
        base.OnComplete();
        _logger.Debug("");
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        _logger.Debug("");
    }
}

