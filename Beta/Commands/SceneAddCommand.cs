using Beta.Scenes;
using Microsoft.Xna.Framework;
using Beta.Entities;
using Beta.DI;
using Beta.Logging;

namespace Beta.Commands;

public class SceneAddCommand : Command
{
    private readonly ILogger _logger;

    public Scene Scene { get; }
    public Entity What { get; }

    public SceneAddCommand(Scene scene, Entity what)
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        Scene = scene;
        What = what;
    }

    public override void Startup()
    {
        What.Scene = Scene;
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